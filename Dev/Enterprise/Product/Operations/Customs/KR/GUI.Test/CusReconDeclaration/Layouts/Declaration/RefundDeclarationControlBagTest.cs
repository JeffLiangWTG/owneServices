using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(RefundDeclarationControlBag))]
	sealed class RefundDeclarationControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(RefundDeclarationControlBag.EntryNumberTextBox);
				yield return nameof(RefundDeclarationControlBag.TotalRefundAmountCalcEdit);
				yield return nameof(RefundDeclarationControlBag.MessageStatusDropEdit);
				yield return nameof(RefundDeclarationControlBag.EntryStatusDropEdit);
				yield return nameof(RefundDeclarationControlBag.AcceptedDateEdit);
				yield return nameof(RefundDeclarationControlBag.RefundApprovalDateEdit);
				yield return nameof(RefundDeclarationControlBag.RefundApprovalNumberTextBox);
				yield return nameof(RefundDeclarationControlBag.ProvisionDateEdit);
				yield return nameof(RefundDeclarationControlBag.ProvisionNumberTextBox);
				yield return nameof(RefundDeclarationControlBag.RefundTypeDropEdit);
				yield return nameof(RefundDeclarationControlBag.RefundCauseDropEdit);
				yield return nameof(RefundDeclarationControlBag.RefundReasonDropEdit);
				yield return nameof(RefundDeclarationControlBag.CustomsOfficeCodeFindBox);
				yield return nameof(RefundDeclarationControlBag.DepartmentCodeFindBox);
				yield return nameof(RefundDeclarationControlBag.TaxOfficeCodeFindBox);
				yield return nameof(RefundDeclarationControlBag.BranchCodeGuidFindBox);
				yield return nameof(RefundDeclarationControlBag.BrokerCodeFindBox);
				yield return nameof(RefundDeclarationControlBag.PayerAddressControl);
				yield return nameof(RefundDeclarationControlBag.RegistrationNumberOneTextBox);
				yield return nameof(RefundDeclarationControlBag.KoreanRegistrationNumberOfCEOTextBox);
				yield return nameof(RefundDeclarationControlBag.PayerBankDropEdit);
				yield return nameof(RefundDeclarationControlBag.BankAccountNumberTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => RefundDeclarationControlBag.Instance;
	}
}
