using System.IO;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.GUI.PlugIn;

public class AECustomsCargoManifestPlugin : CustomsManifestPlugIn
{
	public AECustomsCargoManifestPlugin(ForwardingConsol consol)
		: base(consol)
	{
	}

	ForwardingConsol Consol
	{
		get { return (ForwardingConsol)ManifestProvider; }
	}

	protected override LicenceCheckpoint LicenceCheckPoint
	{
		get { return Env.Licence.ImportManifest; }
	}

	public override string Name
	{
		get { return "Customs Manifest"; }
	}

	protected override void ChangeTheVisibilityCore()
	{
		Enabled = Consol != null
			&& Consol.IsSea
			&& (Consol.IsImport() || Consol.IsCrossTrade() || Consol.IsDomestic());
	}

	protected override MenuItem GetNewTopLevelMenu()
	{
		if (mainMenuItem == null)
		{
			mainMenuItem = new ZMenuItem(Name);
			var menuItemDeclareManifest = new ZMenuItem("Save Customs Manifest");
			menuItemDeclareManifest.Click += new System.EventHandler(DeclareManifest);
			mainMenuItem.MenuItems.Add(menuItemDeclareManifest);
		}

		return mainMenuItem;
	}
	MenuItem mainMenuItem;

	void DeclareManifest(object sender, System.EventArgs e)
	{
		if (string.IsNullOrEmpty(Env.Registry.AECustoms.CourierID))
		{
			Globals.Message.ShowError("Please set a value in Config > System > Registry > Customs > UAE > Clearing Agent Code");
		}
		else if (Consol.HasChanges)
		{
			Globals.Message.ShowError("You must save the current record before generating a message");
		}
		else
		{
			var builder = new ManifestMessageBuilder(Consol);
			if (string.IsNullOrEmpty(builder.Errors))
			{
				using (var dialog = new ZSaveFileDialog())
				{
					dialog.Filter = "Manifest files (*.man)|*.man|All files (*.*)|*.*";
					dialog.CheckPathExists = true;
					dialog.AddExtension = true;
					dialog.OverwritePrompt = true;
					dialog.RestoreDirectory = true;
					dialog.Title = "Save Manifest";

					if (ShowSaveDialog(dialog) == DialogResult.OK)
					{
						using (var writer = new StreamWriter(dialog.OpenFile()))
						{
							writer.Write(builder.GetMessage());
						}
					}
				}
			}
			else
			{
				Globals.Message.ShowError(string.Format("Unable to send due to the following errors:\n\n{0}", builder.Errors));
			}
		}
	}

	protected virtual DialogResult ShowSaveDialog(ZSaveFileDialog dialog)
	{
		return dialog.ShowDialog();
	}

	protected override ZBool HasUserControl
	{
		get { return false; }
	}
}
