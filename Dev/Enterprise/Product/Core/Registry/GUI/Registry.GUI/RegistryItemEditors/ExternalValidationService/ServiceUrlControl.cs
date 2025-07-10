using System;
using System.Threading;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public partial class ServiceUrlControl : RegistryZUserControl
	{
		public ServiceUrlControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			textBoxUrl.ReadOnly = readOnly;
		}

		public string Value
		{
			get { return textBoxUrl.Text; }
			set { textBoxUrl.Text = value; }
		}

		protected virtual async void buttonTestConnection_Click(object sender, EventArgs e)
		{
			Uri serviceUri;
			if (!Uri.TryCreate(textBoxUrl.Text.Trim(), UriKind.Absolute, out serviceUri))
			{
				Globals.Message.ShowError(Res.GetString("139E1506-F033-4EC6-8F7A-77C7BF7C89A3", "The URL is of invalid format."));
				return;
			}
			try
			{
				string testResult = await ExternalValidationServiceClient.GetWebServiceResult(serviceUri, testRequestValue, CancellationToken.None);
				if (String.Equals(testResult, expectedTestResponse, StringComparison.OrdinalIgnoreCase))
				{
					Globals.Message.Show(Res.GetString("EC1CF746-84BA-45C0-95EE-EAB77763768A", "Test successfully."));
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("CA2C5D2C-6B51-4E91-A531-F4182185363E",
						"The URL is not a valid remote validation service."));
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError(Res.GetString("7D2746E3-E13E-4D20-A54B-19BAF05A6894",
					"Failed to connect to the remote validation service."));
				return;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded form parameter value")]
		const string testRequestValue = "Test";
		const string expectedTestResponse = "OrgHeaderValidationService";
	}
}
