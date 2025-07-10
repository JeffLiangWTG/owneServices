using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DrawbackMessageManager : CMRMessageManager
	{
		public DrawbackMessageManager(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}

		internal override ICalculatedCusStatusCalculator[] StatusCalculators => new ICalculatedCusStatusCalculator[] { declaration.calculator };

		internal override string GetStatus() => declaration.JE_MessageStatus;

		protected override void ResetToOriginalCore()
		{
			base.ResetToOriginalCore();
			declaration.JE_MessageStatus = ZString.Empty;
			declaration.JE_EntryStatus = ZString.Empty;
			declaration.DeclarationNumber = ZString.Empty;
			var declarationQuestions = (CMRCusEntryCPDec[])declaration.DrawbackQuestions.ToArray(typeof(CMRCusEntryCPDec));
			foreach (CMRCusEntryCPDec question in declarationQuestions)
			{
				question.Delete();
			}
		}

		internal override CMRAmendmentGenerator GetAmendmentManager(BusinessObject bizo) => null;

		internal override CMRMessageBuilder[] GetBuilder(BusinessObject bizo) => new CMRMessageBuilder[] { new DRWBCKMessageBuilder(bizo as JobDeclaration) };

		public override string MessageFriendlyName => "Drawback: " + ((JobDeclaration)BusinessObject).JE_DeclarationReference;

		public override BusinessObject BusinessObject => declaration;

		internal override EDIMessageCollection GetMessages(BusinessObject bizo) => (bizo as JobDeclaration).Messages;

		protected override ArrayList ValidCanSendOriginalStatusCodes
		{
			get
			{
				ArrayList result = new ArrayList();
				result.Add("NOT");
				result.Add(CustomsEntryStatus.NotSent.Code);
				result.Add(CustomsEntryStatus.FailOriginal.Code);
				result.Add(CustomsEntryStatus.ClearWithdrawal.Code);
				return result;
			}
		}
		readonly JobDeclaration declaration;
	}
}
