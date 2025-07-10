using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsAdditionalInfo : EU.NCTS.Business.NctsAdditionalInfo
	{
		public NctsAdditionalInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZString CSI_SubType
		{
			get => base.CSI_SubType;
			set
			{
				var oldValue = CSI_SubType;
				base.CSI_SubType = value;
				if (!IsCopying && oldValue != value)
				{
					NctsHeaderDocumentsSequenceNumberHelper.ResetAdditionalDocumentsLineNoWhenPhase5((NctsHeader)Header, (Integration.Customs.ICusSupportingInfoTypeSupporter)Parent, oldValue);
					NctsHeaderDocumentsSequenceNumberHelper.ResetAdditionalDocumentsLineNoWhenPhase5((NctsHeader)Header, (Integration.Customs.ICusSupportingInfoTypeSupporter)Parent, value);
				}
			}
		}

		public new EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoLookups Lookups
		{
			get
			{
				if (IsPhase5)
				{
					return (NctsAdditionalInfoPhase5Lookups)base.Lookups;
				}
				else
				{
					return (NctsAdditionalInfoLookups)base.Lookups;
				}
			}
		}

		protected override CusSupportingInfoLookups GetNewPhase4Lookups() => new NctsAdditionalInfoLookups(this);

		public override void OnSaving()
		{
			base.OnSaving();

			if (CSI_LineNo.IsEmpty)
			{
				NctsHeaderDocumentsSequenceNumberHelper.ResetAdditionalDocumentsLineNoWhenPhase5((NctsHeader)Header, (Integration.Customs.ICusSupportingInfoTypeSupporter)Parent, CSI_SubType);
			}
		}

		public override void Delete()
		{
			if (IsInDatabase)
			{
				NctsHeaderDocumentsSequenceNumberHelper.ResetAdditionalDocumentsLineNoWhenPhase5((NctsHeader)Header, (Integration.Customs.ICusSupportingInfoTypeSupporter)Parent, CSI_SubType);
			}
			base.Delete();
		}

		protected override bool AutomaticSequenceNumberEnabled => IsPhase5 && (IsArrival || !IsInPhase5TransitionPeriod);

		protected override bool CSI_LineNo_ReadOnly => true;

		protected override ZString DefaultSubTypeValue => ZString.Empty;

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			if (IsInPhase5TransitionPeriod)
			{
				args.AddExcludedColumns([Schema.CSI_LineNo]);
			}

			return base.CloneInternal(args);
		}

		protected override IAdditionalDocumentReadOnlyProvider GetNewReadOnlyProvider()
		{
			var baseReadOnlyProvider = base.GetNewReadOnlyProvider();
			return IsPhase5Arrival ? new NctsArrivalAdditionalDocumentReadOnlyProvider(this, baseReadOnlyProvider) : baseReadOnlyProvider;
		}
	}
}
