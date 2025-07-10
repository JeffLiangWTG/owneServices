using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.Core.Modules;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Module
{
	public class BMReleaseSequenceModule : ZFilterGridModule
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "part of a Glow Portal link")]
		const string PortalUrl = "/CSQ/Desktop#/";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "part of a Glow Portal link")]
		const string NewReleaseSequenceUrl = "/CSQ/Desktop#/formFlow/2d32cde1-8d3b-4972-b040-0c714b2d2b67";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "part of a Glow Portal link")]
		const string ExistingReleaseSequenceUrl = "/CSQ/Desktop#/formFlow/0d1a7285-c5af-4173-91c5-770b93c5cf83/";

		public BMReleaseSequenceModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID => ModuleIDs.BMReleaseSequence;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.BMReleaseSequence);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new BMReleaseSequenceFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new BMReleaseSequenceFilterControl(GridCollection, (BMReleaseSequenceFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new BMReleaseSequenceCollection(Factory);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.BufferManagement;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.BMReleaseSequence;

		protected override void HandleViewClickCore(object sender, EventArgs e)
		{
			OpenURL(SelectedBusinessObjects);
		}

		protected override void HandleNewClickCore()
		{
			OpenURL();
		}

		protected override void HandleEditClickCore(object sender, EventArgs e)
		{
			OpenURL(SelectedBusinessObjects);
		}

		protected override void HandleDeleteClickCore(object sender, EventArgs e)
		{
			OpenURL(SelectedBusinessObjects);
		}

		internal static protected void OpenURL(BusinessObject[] objects = null)
		{
			var baseUrl = GlowRegistry.Instance.GlowPortalsUri.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

			if (objects == null)
			{
				WebUrlLauncher.Launch(baseUrl + NewReleaseSequenceUrl);
			}
			else
			{
				if (!objects.Any())
				{
					WebUrlLauncher.Launch(baseUrl + PortalUrl);
				}
				else
				{
					foreach (var obj in objects)
					{
						var link = baseUrl + ExistingReleaseSequenceUrl + obj.PK.ToString();
						WebUrlLauncher.Launch(link);

						var recordUri = ShowEditFormUrlHandler.Instance.Create(ControllerIDs.BMReleaseSequence, obj.PK);
						var linkWrapper = new LinkWrapper(ControllerIDs.BMReleaseSequence.Name, obj.PK.ToGuid(), recordUri, obj.HumanReadableShortcutName);
						ObjectFactory.Get<IFavoriteProvider>().AddToRecentItems(linkWrapper);
					}
				}
			}
		}

		#region IModuleDecisionProvider Overrides

		protected override IModuleDecisionProvider CreateDefaultModuleDecisionProvider() => new BMReleaseSequenceModuleDecisionProvider(this);

		class BMReleaseSequenceModuleDecisionProvider : DefaultModuleDecisionProvider
		{
			public BMReleaseSequenceModuleDecisionProvider(ZFilterModule module)
				: base(module)
			{
			}

			public override void HandleDefaultAction(BusinessObject[] selectedBusinessObjects)
			{
				OpenURL(selectedBusinessObjects);
			}
		}

		#endregion
	}
}
