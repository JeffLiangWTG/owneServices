using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public sealed class PrintChargesBilledToLocalClientAtDestAsCollect : AutoPrintChargesBilledToLocalClientAtDestAsCollect
	{
		public PrintChargesBilledToLocalClientAtDestAsCollect()
		{
		}

		public PrintChargesBilledToLocalClientAtDestAsCollect(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		#region Properties

		[List("Lookups.TransportMode_List")]
		public override ZString TransportMode
		{
			[DebuggerStepThrough]
			get { return base.TransportMode; }
			[DebuggerStepThrough]
			set { base.TransportMode = value; }
		}

		[List("Lookups.CountryCollection")]
		public override ZString ExportCountry
		{
			[DebuggerStepThrough]
			get { return base.ExportCountry; }
			[DebuggerStepThrough]
			set { base.ExportCountry = value; }
		}

		[List("Lookups.CountryCollection")]
		public override ZString ImportCountry
		{
			[DebuggerStepThrough]
			get { return base.ImportCountry; }
			[DebuggerStepThrough]
			set { base.ImportCountry = value; }
		}

		#endregion

		#region Validation

		public override void ValidateTransportMode()
		{
			base.ValidateTransportMode();
			MandatoryValidation.CheckEntered(TransportModeInfo);
			ListValidation.ErrorIfInvalidCode(TransportModeInfo, Lookups.TransportMode_List);

			if (ParentCollections.Count > 0 && !TransportMode.IsEmpty && !IsRowUnique())
			{
				TransportModeInfo.AddError(UniqueRowErrorMessage);
			}
		}

		public override void ValidateExportCountry()
		{
			base.ValidateExportCountry();
			ListValidation.ErrorIfInvalidCode(ExportCountryInfo, Lookups.CountryCollection);

			if (ParentCollections.Count > 0 && !IsRowUnique())
			{
				ExportCountryInfo.AddError(UniqueRowErrorMessage);
			}
		}

		public override void ValidateImportCountry()
		{
			base.ValidateImportCountry();
			ListValidation.ErrorIfInvalidCode(ImportCountryInfo, Lookups.CountryCollection);

			if (ParentCollections.Count > 0 && !IsRowUnique())
			{
				ImportCountryInfo.AddError(UniqueRowErrorMessage);
			}
		}

		#endregion

		#region Lookups

		public PrintChargesBilledToLocalClientAtDestAsCollectLookups Lookups
		{
			get { return lookups ?? (lookups = new PrintChargesBilledToLocalClientAtDestAsCollectLookups(this, CurrentFactory)); }
		}
		PrintChargesBilledToLocalClientAtDestAsCollectLookups lookups;

		#endregion

		#region Implementation

		bool IsRowUnique()
		{
			var parentCollection = (PrintChargesBilledToLocalClientAtDestAsCollectCollection)GetParentCollection(this, typeof(PrintChargesBilledToLocalClientAtDestAsCollectCollection));

			return parentCollection == null || !parentCollection
						.Cast<PrintChargesBilledToLocalClientAtDestAsCollect>()
						.Any(y =>
							y.PK != PK
							&& !y.IsDeleted
							&& y.TransportMode == TransportMode
							&& y.ExportCountry == ExportCountry
							&& y.ImportCountry == ImportCountry);
		}

		ZString UniqueRowErrorMessage
		{
			get
			{
				return Res.GetString("9B892898-5921-4D09-B336-1EC8F5E00EE8", "The combination of Transport Mode, Export Country and Import Country may only appear once in this list.");
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PrintChargesBilledToLocalClientAtDestAsCollect(fallbackLevel);
		}

		#endregion
	}
}


