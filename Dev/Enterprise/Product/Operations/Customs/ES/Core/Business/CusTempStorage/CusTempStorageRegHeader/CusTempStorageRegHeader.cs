using System;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Business.CusTempStorage
{
	public class CusTempStorageRegHeader : EU.TemporaryStorage.Business.CusTempStorageRegHeader, Integration.Customs.ES.ICusTempStorageRegHeader
	{
		public CusTempStorageRegHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		public const string ESAppCode = "ADT";

		#region Override Properties

		[ReadOnly(true)]
		public override ZString SRH_Reference
		{
			get => base.SRH_Reference;
			set => base.SRH_Reference = value;
		}

		[ReadOnly(true)]
		public override ZString SRH_InternalReference
		{
			get => base.SRH_InternalReference;
			set => base.SRH_InternalReference = value;
		}

		[ReadOnly(true)]
		public override ZDate SRH_ArrivalDate
		{
			get => base.SRH_ArrivalDate;
			set => base.SRH_ArrivalDate = value;
		}

		[ReadOnly(true)]
		public override ZDateTime SRH_PresentationDate
		{
			get => base.SRH_PresentationDate;
			set => base.SRH_PresentationDate = value;
		}

		[ReadOnly(true)]
		public override ZString SRH_PreviousReferenceType
		{
			get => base.SRH_PreviousReferenceType;
			set => base.SRH_PreviousReferenceType = value;
		}

		[ReadOnly(true)]
		public override ZString SRH_PreviousReference
		{
			get => base.SRH_PreviousReference;
			set => base.SRH_PreviousReference = value;
		}

		[ReadOnly(true)]
		public override ZString SRH_Status
		{
			get => base.SRH_Status;
			set => base.SRH_Status = value;
		}

		#endregion

		public ZString TSDStatusURL => Factory.GetCached(ref tsdStatusURL, () =>
		{
			var tsdStatusURL = ZString.Empty;
			var reference = SRH_Reference;

			if (reference.Length < 18)
			{
				tsdStatusURL = GetTSDStatusURL(reference.SubstringSafe(0, 4), reference.SubstringSafe(4, 1), reference.SubstringSafe(5), ZString.Empty);
			}
			else
			{
				tsdStatusURL = GetTSDStatusURL(ZString.Empty, ZString.Empty, ZString.Empty, reference);
			}
			return tsdStatusURL;

			static ZString GetTSDStatusURL(ZString recinto, ZString anio, ZString numero, ZString mrn)
			{
				var url = ESCustomsDataRegistry.Instance.SummaryDeclarationStatusQueryURL.Value;
				return url.Replace("%recinto%", recinto).Replace("%anio%", anio).Replace("%numero%", numero).Replace("%mrn%", mrn);
			}
		});
		CachedProperty<ZString> tsdStatusURL;

		public new CusTempStorageRegHeaderLookups Lookups => (CusTempStorageRegHeaderLookups)base.Lookups;

		protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeaderLookups GetNewLookups() => new CusTempStorageRegHeaderLookups(this);

		public new CusTempStorageRegLineCollection CusTempStorageRegLines => (CusTempStorageRegLineCollection)base.CusTempStorageRegLines;

		protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineCollection CreateNewCusTempStorageRegLines()
		{
			var lines = new CusTempStorageRegLineCollection(this);
			lines.SetReadOnlyIncludingChildren(true);
			lines.OnLoadedIntoCollection = line => line.CusTempStorageRegLineTransactions.SetReadOnlyIncludingChildren(line.SRL_CustomsStatus != TempStorageDeclarationStatusList.Codes.Open || !line.HasRegLineOBLTransaction());
			return lines;
		}

		protected override Type GetStorageRegLineTypeCore() => typeof(CusTempStorageRegLine);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SRH_AppCode = ESAppCode;
		}

		public override void Delete()
		{
			base.Delete();
			this.DeleteChildren<CusTempStorageRegLine>(CusTempStorageRegLineSchema.SRL_SRH);
		}
	}
}
