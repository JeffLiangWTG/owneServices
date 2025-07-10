using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsSupportingDocument : EU.NCTS.Business.NctsSupportingDocument
	{
		public NctsSupportingDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set
			{
				var oldValue = CSI_Code;
				base.CSI_Code = value;
				if (!IsCopying && oldValue != value)
				{
					NctsHeaderDocumentsSequenceNumberHelper.ResetSupportingDocumentsLineNoWhenPhase5((NctsHeader)Header, (Integration.Customs.ICusSupportingInfoTypeSupporter)Parent);
				}
			}
		}

		protected override CusSupportingInfoValidation GetNewPhase5Validation() => IsPhase5Departure ? base.GetNewPhase5Validation() : new NctsSupportingDocumentPhase5ArrivalValidation(this);

		protected override CusSupportingInfoValidation GetNewPhase4Validation() => new NctsSupportingDocumentPhase4Validation(this);

		public override void OnSaving()
		{
			base.OnSaving();

			if (CSI_LineNo.IsEmpty)
			{
				NctsHeaderDocumentsSequenceNumberHelper.ResetSupportingDocumentsLineNoWhenPhase5((NctsHeader)Header, (Integration.Customs.ICusSupportingInfoTypeSupporter)Parent);
			}
		}

		public override void Delete()
		{
			if (IsInDatabase)
			{
				NctsHeaderDocumentsSequenceNumberHelper.ResetSupportingDocumentsLineNoWhenPhase5((NctsHeader)Header, (Integration.Customs.ICusSupportingInfoTypeSupporter)Parent);
			}
			base.Delete();
		}

		protected override bool AutomaticSequenceNumberEnabled => IsPhase5Arrival || (IsPhase5 && !IsInPhase5TransitionPeriod);

		protected override bool CSI_LineNo_ReadOnly => true;

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			if (IsInPhase5TransitionPeriod)
			{
				args.AddExcludedColumns([Schema.CSI_LineNo]);
			}

			return base.CloneInternal(args);
		}

		protected override ISupportingDocumentReadOnlyConditions GetNewReadOnlyProvider()
		{
			var baseReadOnlyProvider = base.GetNewReadOnlyProvider();
			return IsPhase5Arrival ? new NctsSupportingDocumentPhase5ArrivalReadOnlyProvider(this, baseReadOnlyProvider) : baseReadOnlyProvider;
		}
	}
}
