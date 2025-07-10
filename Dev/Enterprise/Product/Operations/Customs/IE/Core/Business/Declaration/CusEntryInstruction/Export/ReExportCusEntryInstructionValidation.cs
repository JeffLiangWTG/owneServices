
namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ReExportCusEntryInstructionValidation : CommonExportCusEntryInstructionValidation
	{
		public ReExportCusEntryInstructionValidation(CusEntryInstruction parent) : base(parent)
		{
		}

		protected override void CheckCEI_Style()
		{
			base.CheckCEI_Style();
			CheckForRequiredTransportDocuments();
		}

		void CheckForRequiredTransportDocuments()
		{
			var parent = Parent;

			if (!parent.HasTransportDocument && !parent.HasInvoiceHeaderWithTransportDocument)
			{
				parent.AddRowMessageError(Res.GetString("438FA985-75F0-4DB5-91DF-32ED001BD1C2", "Please provide at least one Transport Document. Transport Documents can be entered on the Additional Documents Tab. Select Kind = TRA"));
			}
		}
	}
}
