using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.EUExitControl;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitSeal : CusSeal, ICusExitSeal
		, IUcc6ValueProvider
	{
		public CusExitSeal(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static new readonly CusSealTypeDecider TypeDecider = new CusSealTypeDecider();

		public static ZQuery GetZQuery(ZGuid pk)
		{
			var query = new ZQuery(CusSealSchema.BK_ParentID, pk);
			query.AddToFilter(CusSealSchema.BK_ParentTableCode, CusExitContainerSchema.Constants.Prefix);
			return query;
		}

		public static CusExitSeal Load(CusExitContainer container, ZShort sequenceNumber)
		{
			CusExitSeal result = null;
			if (container != null)
			{
				var query = GetZQuery(container.PK);
				query.FetchOnlyFromLocalCache = !container.IsInDatabase;
				result = container.Factory.Load<CusExitSeal>(query).Where(x => x.BK_SequenceNumber == sequenceNumber).OrderBy(x => x.BK_SystemCreateTimeUtc).FirstOrDefault();
			}
			return result;
		}

		public static CusExitSeal LoadOrCreate(CusExitContainer container, ZShort sequenceNumber)
		{
			CusExitSeal result = null;
			if (container != null)
			{
				result = Load(container, sequenceNumber) ?? Create(container, sequenceNumber);
			}
			return result;
		}

		public static CusExitSeal Create(CusExitContainer container, ZShort sequenceNumber)
		{
			var result = container.Factory.New<CusExitSeal>();
			result.BK_ParentID = container.PK;
			result.BK_ParentTableCode = CusExitContainerSchema.Constants.Prefix;
			result.BK_SequenceNumber = sequenceNumber;
			return result;
		}

		[ResourceStringData("{405F64B1-9CDA-4D91-8A07-AD5C3C6CA61E}", Caption = "Number")]
		public override ZString BK_SealNumber { get => base.BK_SealNumber; set => base.BK_SealNumber = value; }

		[ResourceStringData("{6948726A-3485-403D-B71F-DC49CC524CD8}", Caption = "Sequence Number", MediumCaption = "Seq Number", ShortCaption = "Seq Num.", FullDescription = "Seal Sequence Number")]
		public override ZShort BK_SequenceNumber { get => base.BK_SequenceNumber; set => base.BK_SequenceNumber = value; }

		[ResourceStringData("2C442679-F393-4980-B5B7-ADE7C107884E", Caption = "Status", MediumCaption = "Status", ShortCaption = "Status", FullDescription = "Seal Status")]
		[List(nameof(Lookups) + "." + nameof(CusExitSealLookups.StatusList))]
		public override ZString BK_UnloadingState { get => base.BK_UnloadingState; set => base.BK_UnloadingState = value; }

		public CusExitContainer Container => Factory.Load<CusExitContainer>(BK_ParentID);

		public new CusExitSealValidation Validation => (CusExitSealValidation)base.Validation;

		public new CusExitSealLookups Lookups => (CusExitSealLookups)base.Lookups;

		public bool IsUCC6 => IsUCC6Core;
		protected virtual bool IsUCC6Core => Container?.IsUCC6 ?? false;

		protected override CusSealValidation GetNewValidation() => new CusExitSealValidation(this);

		protected override CusSealLookups GetNewLookups() => IsUCC6
			? new CusExitSealUcc6Lookups(this)
			: new CusExitSealLookups(this);

		public ICusExitSealValidationDecider ValidationDecider => CachedValueHelper.GetValue(ref validationDeciderCached, GetValidationDecider);
		CachedValue<ICusExitSealValidationDecider> validationDeciderCached;

		ICusExitSealValidationDecider GetValidationDecider() => Container?.Header?.Configuration.CusExitSealConfiguration.GetValidationDecider(this);
	}
}
