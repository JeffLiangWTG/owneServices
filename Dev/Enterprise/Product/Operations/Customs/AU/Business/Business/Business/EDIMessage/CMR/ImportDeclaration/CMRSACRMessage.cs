using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRSACRMessage : CMRImportDeclarationMessage, IOutstandingPaymentInfoProvider
	{
		public CMRSACRMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.SAC;
		}

		protected override internal BusinessObject GetWrappedObject()
		{
			BusinessObject result = null;
			ZString reference = GetReferenceFromSendersReference(SendersReference);
			result = CusEntryHeader.LoadForBGMReference(Factory, reference);
			if (result == null)
			{
				result = base.GetWrappedObject();
			}

			return result;
		}

		protected override string GetAdditionalMessageAdviceForThisError(string errorCode)
		{
			ZStringBuilder result = new ZStringBuilder();
			result.Append(base.GetAdditionalMessageAdviceForThisError(errorCode));

			switch (errorCode)
			{
				case "ID0853":
					result.Append(SACWithLineAdvice);
					break;
			}
			return result.ToString();
		}

		public override bool SupportsHTMLResponseEmails
		{
			get { return true; }
		}

		public const string SACWithLineAdvice = "A SAC with lines is specificaly for Low value declarations containing Alcohol and Tobacco. A SAC with lines cannot be used for any other purpose. You have to lodge as IMD and Customs will convert the entry to SAC at their end.";

		#region IOutstandingPaymentInfoProvider Members

		SegmentGroup5MessageSection IOutstandingPaymentInfoProvider.Group5Section
		{
			get { return CUSRES.Group5; }
		}

		GISSegmentMessageSection IOutstandingPaymentInfoProvider.GISSection
		{
			get { return CUSRES.GIS; }
		}

		#endregion
	}
}
