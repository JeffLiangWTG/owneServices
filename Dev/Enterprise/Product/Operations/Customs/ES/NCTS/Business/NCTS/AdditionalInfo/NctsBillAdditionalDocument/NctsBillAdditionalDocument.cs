using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsBillAdditionalDocument : EU.NCTS.Business.NctsBillAdditionalDocument
	{
		public NctsBillAdditionalDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		NctsBill Bill => Parent as NctsBill;

		bool IsPhase5 => Bill?.Header?.IsPhase5 ?? false;

		bool IsPhase5Arrival => Bill?.Header?.IsPhase5Arrival ?? false;

		public override ZString CSI_SubType
		{
			get => base.CSI_SubType;
			set
			{
				var oldValue = CSI_SubType;
				base.CSI_SubType = value;
				if (!IsCopying && oldValue != value)
				{
					NctsHeaderDocumentsSequenceNumberHelper.ResetAdditionalDocumentsLineNoWhenPhase5(Bill.Header, Bill, oldValue);
					NctsHeaderDocumentsSequenceNumberHelper.ResetAdditionalDocumentsLineNoWhenPhase5(Bill.Header, Bill, value);
				}
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (CSI_LineNo.IsEmpty)
			{
				NctsHeaderDocumentsSequenceNumberHelper.ResetAdditionalDocumentsLineNoWhenPhase5(Bill.Header, Bill, CSI_SubType);
			}
		}

		public override void Delete()
		{
			if (IsInDatabase)
			{
				NctsHeaderDocumentsSequenceNumberHelper.ResetAdditionalDocumentsLineNoWhenPhase5(Bill.Header, Bill, CSI_SubType);
			}
			base.Delete();
		}

		protected override bool AutomaticSequenceNumberEnabled => IsPhase5Arrival || (IsPhase5 && !IsTransitPeriod);

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			if (IsTransitPeriod)
			{
				args.AddExcludedColumns([Schema.CSI_LineNo]);
			}

			return base.CloneInternal(args);
		}

		bool IsTransitPeriod => Bill?.IsInPhase5TransitionPeriod ?? false;

		protected override IAdditionalDocumentReadOnlyProvider GetNewReadOnlyProvider()
		{
			var baseReadOnlyProvider = base.GetNewReadOnlyProvider();
			return IsArrivalMovement
				? new NctsBillsArrivalAdditionalDocumentReadOnlyProvider(this, baseReadOnlyProvider)
				: baseReadOnlyProvider;
		}
	}
}
