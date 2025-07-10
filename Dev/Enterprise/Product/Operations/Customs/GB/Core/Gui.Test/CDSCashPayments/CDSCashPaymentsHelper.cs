using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.CDSCashPayments.Testing
{
	static class CDSCashPaymentsHelper
	{
		public static Form GetFormForTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var payInfo = cusEntryHeader.EntryPayInfos.AddNew();
			payInfo.C9_PaymentDate = ZDateTime.Now;
			payInfo.C9_PaymentAmount = 100m;
			payInfo.C9_PaymentReference = "GB1Reference";
			payInfo.C9_PaymentStatus = Customs.Business.CusEntryPayInfoStatusList.Codes.Clear;
			payInfo.C9_ReceiptDate = ZDate.Today;
			factory.Save();
			return new CDSCashPaymentsForm(payInfo as CusEntryPayInfo);
		}

		public static T GetSingleControlOrNull<T>(this Control parent, string controlName) where T : Control
		{
			T control;
			try
			{
				control = parent.FindSingle<T>(controlName);
			}
			catch (Exception ex) when (ex is ArgumentNullException || ex is InvalidOperationException)
			{
				control = null;
			}
			return control;
		}
	}
}
