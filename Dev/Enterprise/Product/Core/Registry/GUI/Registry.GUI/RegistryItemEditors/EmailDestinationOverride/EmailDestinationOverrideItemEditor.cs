using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.GUI
{
	public class EmailDestinationOverrideItemEditor : TextBoxRegistryItemEditor
	{
		public EmailDestinationOverrideItemEditor(IRegistryDataType dataType, BusinessObjectFactory factory, TextRegistryEditorInfo info)
		: base(dataType, info)
		{
			this.factory = factory;
		}

		protected override void AddButtonClickEvent(ZUserControl control)
		{
			var clearButton = new ZButton()
			{
				CaptionResourceString = Res.GetData("9F87B504-B1F7-4659-8569-714B3E74D4D5", "Clear NDR Status"),
				Size = ControlDpiScalingHelper.NewScaledSize(100, 25, true),
				Location = ControlDpiScalingHelper.NewScaledPoint(0, 50, true)
			};
			control.Controls.Add(clearButton);

			clearButton.Click += new System.EventHandler(this.Button_Click);

			control.SetDataBinding(Wrapper, "");
		}

		void Button_Click(object sender, EventArgs e)
		{
			var currentEmailAddress = this.Wrapper.Value.ToString();
			var address = Env.Registry.EmailDestinationOverride;

			if (currentEmailAddress != address)
			{
				Globals.Message.Show(Res.GetString("74A274B5-AE9E-4D3D-8B51-AFEE3AD28655", "Please save the data first."));
			}
			else
			{
				var query = new ZDBOnlyQuery(typeof(GlbEmailAddress));
				query.AddToFilter(GlbEmailAddressSchema.GI_DeliveryStatus, EmailDeliveryReportStatus.Codes.NonDeliveryReport);
				query.AddToFilter(GlbEmailAddressSchema.GI_EmailAddress, address);

				var emailAddress = factory.Load<GlbEmailAddress>(query).FirstOrDefault();
				if (emailAddress == null)
				{
					Globals.Message.Show(Res.GetString("9E063E46-88E7-4C18-A4B9-1E452BC3EF7E", "Email Destination Override is not NDR."));
				}
				else
				{
					emailAddress.GI_DeliveryStatus = ZString.Empty;

					try
					{
						factory.Save();
						Globals.Message.Show(Res.GetString("10838524-38FA-479D-A7C9-6315D447D37D", "The NDR status has been cleared successfully."));
					}
					catch (ZSaveException)
					{
					}
				}
			}
		}

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;

		readonly BusinessObjectFactory factory;

		TextBoxWrapper Wrapper
		{
			get
			{
				if (wrapper == null)
				{
					wrapper = new TextBoxWrapper();
				}
				return wrapper;
			}
		}
		TextBoxWrapper wrapper;
	}
}
