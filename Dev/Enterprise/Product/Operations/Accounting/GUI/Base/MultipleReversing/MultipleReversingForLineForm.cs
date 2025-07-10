using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.GUI.Base
{
	public partial class MultipleReversingForLineForm : MultipleReversingBaseForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public MultipleReversingForLineForm() : base()
		{
			InitializeComponent();
		}

		public MultipleReversingForLineForm(MultipleReversingProviderForLine multipleReversingProvider)
			: base(multipleReversingProvider)
		{
			InitializeComponent();
		}

		MultipleReversingProviderForLine MultipleReversingProvider
		{
			get { return (MultipleReversingProviderForLine)BusinessEntity; }
		}

		public override string FormCaption => Res.GetString("6de50f40-6023-41cc-b4fe-3a26baf0a638", "Multiple WIP/Accruals");

		public override string FormVerb => Res.GetString("e83df97d-27f7-42eb-a77b-50fab20f2438", "Reverse");

		protected override ResourceStringData RemoveErrorTransactionsButtonCaption => Res.GetData("e97ed1e4-b0df-4a44-9d62-49abaa1bb1a0", "Remove WIP/Accruals That Can\'t Be Reversed");

		protected override bool IsReversingMode => DisplayMode == ODisplayMode.Delete;

		protected override List<string> GetEditableFields(BusinessObject header) => new List<string>() { AccTransactionLines.Schema.AL_ReverseDate };

		protected override ContinueWithDelete ShowPreDeleteDialogs()
		{
			var result = ContinueWithDelete.No;

			if (MultipleReversingProvider.TransactionLinesAlreadyReversed.Any())
			{
				MultipleReversingProvider.RunPreSaveValidation();
				if (MultipleReversingProvider.HasErrors)
				{
					ShowErrorsDialog();
				}
				else
				{
					result = base.ShowPreDeleteDialogs();
				}
			}
			return result;
		}

		protected override DialogResult ShowConfirmationForDelete()
		{
			var message = Res.GetString("4982a135-dd7a-4851-bc39-4c89b854fb88", "This will reverse all listed WIPs and Accruals in this grid.") + "\r\n";
			message += Res.GetString("d5321c3e-427d-4aba-aae1-35e765145069", "Are you sure you want to continue?");
			var caption = Res.GetString("b01df586-c7ea-4678-8286-71e30fa4607e", "Reverse All");
			return Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No);
		}
	}
}
