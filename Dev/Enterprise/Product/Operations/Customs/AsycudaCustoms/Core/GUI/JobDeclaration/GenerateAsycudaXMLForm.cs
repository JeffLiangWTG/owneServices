using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	public partial class GenerateAsycudaXMLForm : MessageSendingObjectForm
	{
		public GenerateAsycudaXMLForm()
		{
		}

		public GenerateAsycudaXMLForm(JobDeclarationMessageSendingObjectParent<JobDeclarationMessageSendingObject> declarationWrapper)
				: base(declarationWrapper)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeNewColumns();
		}

		void InitializeNewColumns()
		{
			var columnStyleInfos = new Core.Forms.ZGridColumnInfo[]
			{
				new ZTextBoxColumnStyleInfo()
				{
					ColumnName = JobDeclarationMessageSendingObject.Schema.LocalReferenceNumber,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					IsMandatory = true,
					CaptionResourceString = Enterprise.Customs.AsycudaCustoms.GUI.Res.GetData("D449F6E5-DDC8-49D2-9721-22D954795AA4", "Reference Number"),
				},
				new ZTextBoxColumnStyleInfo()
				{
					ColumnName = JobDeclarationMessageSendingObject.Schema.DeclarationType,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					IsMandatory = true,
				},
			};

			MessageSendingObjectsGrid.ColumnStyles.AddRange(columnStyleInfos);
		}

		public override string FormHeading => Enterprise.Customs.AsycudaCustoms.GUI.Res.GetString("330AE86C-8A21-4999-84E6-D70DB5A4FC0B", "Generate Asycuda XML Form");
	}
}
