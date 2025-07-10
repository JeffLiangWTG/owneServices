using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class PriceTierLicenceSetting : PriceLicenceSetting
	{
		public PriceTierLicenceSetting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			LS9_Type = BillingConstants.LicenceSetting.PriceTier;
		}

		[List(nameof(Lookups) + "." + nameof(EdiLicenceSettingLookups.PriceTierCodesForCategory))]
		public override ZString PriceCode { get => base.PriceCode; set => base.PriceCode = value; }

		public override bool SupportUnitBreak => true;

		public override (ZDecimal? Price, ZDecimal? Units) GetPriceAndUnits(ZInt unitBreak)
		{
			var line = Lines.OfType<PriceTierLicenceSettingLine>().FirstOrDefault(x => x.UnitBreak == unitBreak);
			return (line?.Price, line?.Units);
		}

		public override (ZDecimal? Price, ZDecimal? Units) GetPriceAndUnits()
			=> throw new NotImplementedException();

		public override ZString Summary => LS9_Name;

		#region Setting Lines

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			var xml = string.Empty;
			if (Lines.Any())
			{
				var serializer = ZXmlSerializer.New(typeof(PriceTierLicenceSettingLineCollection));
				using (var writer = new StringWriter(CultureInfo.InvariantCulture))
				{
					serializer.Serialize(writer, Lines);
					xml = writer.ToString();
				}
			}
			LoadOrCreateHiddenStmNote().ST_NoteDataAsText = xml;
		}

		HiddenStmNote LoadOrCreateHiddenStmNote()
		{
			var filter = new ZQuery(StmNoteSchema.ST_Description, BillingConstants.LicenceSetting.PriceTier);
			filter.AddToFilter(StmNoteSchema.ST_ParentID, PK);
			filter.AddToFilter(StmNoteSchema.ST_Table, TableName);
			var note = Factory.LoadTop1<HiddenStmNote>(filter);

			if (note == null)
			{
				note = Factory.New<HiddenStmNoteNotAutoLogged>();
				note.ST_Description = BillingConstants.LicenceSetting.PriceTier;
				note.ST_Table = TableName;
				note.ST_ParentID = PK;
			}
			return note;
		}

		[ChildEditable]
		public PriceTierLicenceSettingLineCollection Lines
		{
			get
			{
				if (lines == null)
				{
					var note = LoadOrCreateHiddenStmNote();
					if (note.ST_NoteDataAsText.IsEmpty)
					{
						lines = new PriceTierLicenceSettingLineCollection();
					}
					else
					{
						var serializer = ZXmlSerializer.New(typeof(PriceTierLicenceSettingLineCollection));
						using (var reader = new StringReader(note.ST_NoteDataAsText))
						{
							lines = (PriceTierLicenceSettingLineCollection)serializer.Deserialize(reader);
							lines.HasChanges = false;
						}
					}

					RegisterEditableChildObject(lines);
				}
				return lines;
			}
		}
		PriceTierLicenceSettingLineCollection lines;

		#endregion
	}

	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class PriceTierLicenceSettingLine : AutoPriceTierLicenceSettingLine
	{
		public PriceTierLicenceSettingLine()
		{
		}

		public override void ValidateUnitBreak()
		{
			base.ValidateUnitBreak();
			MandatoryValidation.CheckNotNegative(UnitBreakInfo);

			if (ParentCollections.Any(x => x.OfType<PriceTierLicenceSettingLine>()
									 .Any(y => y.PK != PK && y.UnitBreak == UnitBreak)))
			{
				UnitBreakInfo.AddError("Duplicate Unit Break not allowed.");
			}
		}
	}

	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class PriceTierLicenceSettingLineCollection : NonPersistentBusinessObjectCollection<PriceTierLicenceSettingLine>
	{
		public PriceTierLicenceSettingLineCollection()
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PriceTierLicenceSettingLine();
		}
	}
}
