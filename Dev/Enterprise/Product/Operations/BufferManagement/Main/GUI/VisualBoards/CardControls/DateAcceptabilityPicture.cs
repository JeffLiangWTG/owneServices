using System;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public partial class DateAcceptabilityPicture : ZUserControl
	{
		public DateAcceptabilityPicture(ITaskCardComponentParent componentParent)
		{
			parent = componentParent;
			SetToolTip();
		}

		readonly ITaskCardComponentParent parent;

		void SetPicture()
		{
			var dateAcceptability = parent.CardContent != null ? parent.CardContent.GetCustomAttribute<ZString>(StaticControlProperty.ApplicableDateAcceptability) : ZString.Empty;

			BackgroundImage = DateAcceptabilityImages.Get(dateAcceptability);
			BackgroundImageLayout = ImageLayout.Stretch;
			Visible = BackgroundImage != null;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (!Disposing && !IsDisposed)
			{
				SetPicture();
			}
		}

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);

			if (!parent.IsPreview)
			{
				ShowDateAcceptabilityLegend();
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1046", Justification = "ToolTip must be set this way, otherwise it doesn't work. Not Good!")]
		void SetToolTip()
		{
			var task = parent.Task;
			var workflow = task != null ? task.GetProcessHeader() : null;

			if (workflow != null)
			{
				var acceptability = workflow.ApplicableDateAcceptability;
				var message = Res.GetString("213af3b0-4930-4730-8120-994ad5c2abda", "Right-click for more information...");

				var agreedDeliveryDate = workflow.AgreedDeliveryDateLocal.IsValid
					? workflow.AgreedDeliveryDateLocal
					: workflow.JobHeader.AgreedDeliveryDateLocal;

				if (agreedDeliveryDate.IsValid)
				{
					message = Res.GetString("8a15f338-c8be-487f-91b1-2823cb03cb9b", "Agreed delivery date: {0}\r\n{1}", agreedDeliveryDate.ToShortDateString(), message);
				}

				var doNoStartBeforeDate = workflow.FH_DoNotStartBeforeDate.IsValid
					? workflow.FH_DoNotStartBeforeDate
					: workflow.JobHeader.FH_DoNotStartBeforeDate;

				if (doNoStartBeforeDate.IsValid)
				{
					message = Res.GetString("01753e42-cb3a-4f5d-b0d0-e1efb8c143d1", "Do not start before: {0}{1}{2}", doNoStartBeforeDate.ToShortDateString(), System.Environment.NewLine, message);
				}

				var tooltip = acceptability.IsEmpty
					? message
					: FormattableString.Invariant($@"{workflow.Lookups.DateAcceptabilities.GetDescriptionFromCode(acceptability)}

{message}"); // Using FormattableString, which should make this okay. Code sniffer defect? SAD!

				ToolTipService.SetToolTip(this, tooltip);
			}
		}

		public void ShowDateAcceptabilityLegend()
		{
			ZFormModaliser.ShowDialogAndDispose(new DateAcceptabilityLegendForm());
		}
	}
}
