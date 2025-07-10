using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Res = Enterprise.Customs.ASYCUDA.Gui.Res;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public class AsycudaConsolPlugin : CustomsCargoManifestPlugin
	{
		public AsycudaConsolPlugin(ForwardingConsol hostBusinessEntity)
			: base(hostBusinessEntity)
		{
			hostBusinessEntity.Factory.Saved += new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
		}

		public static class Constants
		{
			public static class Message
			{
				public static string MutexLockText(ZString lockByInformation) => Res.GetString("AsycudaConsolPlugin|MutexLockText", "{0} is already in the process of creating a Customs Manifest for this Consol.\r\nYou should be able to access the Customs Manifest when the person has saved the record. Please try later.", lockByInformation);
				public static string ManifestHasAlreadyBeenCreated => Res.GetString("AsycudaConsolPlugin|ManifestHasAlreadyBeenCreated", "The Customs Manifest for this Consol has already been added by another user.\r\nPlease close and re-open the Consol to see the newly added Manifests.");
			}
		}

		public ManifestHeadersWrapper HeaderWrapper => BusinessEntity as ManifestHeadersWrapper;

		public override ZString PlugInNotDisplayedMessage => coveringLabelText;
		ZString coveringLabelText;

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					UnlockMutexIfLockedByThisInstance();
					HostBusinessEntity.Factory.Saved -= Factory_Saved;
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		protected override string NameCore => "Manifest";

		protected new ForwardingConsol HostBusinessEntity => (ForwardingConsol)base.HostBusinessEntity;

		protected override bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated() => QueryAddManifestHeader();

		protected override IBusiness GetBusinessEntityForPlugIn() => new ManifestHeadersWrapper(HostBusinessEntity);

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.Core;

		protected override Control GetNewUserControl() => fMessageUserControl ?? (fMessageUserControl = new AsycudaManifestMainControl());
		AsycudaManifestMainControl fMessageUserControl;

		protected override MenuItem GetNewTopLevelMenu() => asycudaMainMenuItem ?? (asycudaMainMenuItem = new AsycudaMenu(HeaderWrapper));
		AsycudaMenu asycudaMainMenuItem;

		protected new ForwardingConsol ManifestProvider
		{
			get { return (ForwardingConsol)base.ManifestProvider; }
		}

		protected override void ChangeTheVisibilityCore()
		{
			var shouldBeEnabled = false;
			if (ManifestProvider != null && !ManifestProvider.IsDeleted)
			{
				shouldBeEnabled = HeaderWrapper.GetConsolCountriesAndTransportModesThatMightNeedManifest().Count > 0;
			}
			Enabled = shouldBeEnabled;
		}

		protected override CustomsManifestStatus GetCustomsManifestStatus(IManifestProvider manifestProvider)
		{
			return null;
		}

		bool QueryAddManifestHeader()
		{
			var result = true;
			coveringLabelText = string.Empty;

			if (!HeaderWrapper.Headers.Any())
			{
				var manifestCountries = HeaderWrapper.GetConsolCountriesAndTransportModesThatMightNeedManifest().Keys;
				var manifestHeaderHasBeenCreated = ManifestHeaderHasBeenCreated(manifestCountries);
				if (Mutex.IsLocked && !manifestHeaderHasBeenCreated)
				{
					UnlockMutexIfLockedByThisInstance();
				}

				if (!Mutex.IsLocked)
				{
					if (!manifestHeaderHasBeenCreated)
					{
						if (Mutex.Lock())
						{
							manifestHeaderHasBeenCreated = ManifestHeaderHasBeenCreated(manifestCountries);
							if (!manifestHeaderHasBeenCreated)
							{
								(UserControl as AsycudaManifestMainControl)?.LoadCountryTabs();
								HeaderWrapper.WR_CountryCode = MostAppropriateManifest(manifestCountries);
							}
						}
						else
						{
							coveringLabelText = Constants.Message.MutexLockText(Mutex.GetMutexLockByInfo());
							result = false;
						}
					}

					if (manifestHeaderHasBeenCreated)
					{
						coveringLabelText = Constants.Message.ManifestHasAlreadyBeenCreated;
						result = false;
					}
				}
				else
				{
					coveringLabelText = Constants.Message.MutexLockText(Mutex.GetMutexLockByInfo());
					result = false;
				}
			}

			return result;
		}

		bool ManifestHeaderHasBeenCreated(IEnumerable<ZString> manifestCountries) => manifestCountries.Any(x => HostBusinessEntity.CheckManifestHeaderHasBeenCreated(x));

		ZString MostAppropriateManifest(IEnumerable<ZString> manifestCountries)
		{
			var result = ZString.Empty;
			if (manifestCountries.Count() == 1)
			{
				result = manifestCountries.First();
			}
			else
			{
				var countryOfLoadPort = HostBusinessEntity.JK_RL_NKLoadPort.Left(2);
				var countryOfDischargePort = HostBusinessEntity.JK_RL_NKDischargePort.Left(2);

				var countries = HostBusinessEntity.IsExport()
						? new[] { countryOfLoadPort, countryOfDischargePort }
						: new[] { countryOfDischargePort, countryOfLoadPort };

				result = countries.FirstOrDefault(country => manifestCountries.Any(c => c == country));
			}

			return HeaderWrapper.CorrectCountryCodeIfNeeded(result);
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				UnlockMutexIfLockedByThisInstance();
			}
		}

		void UnlockMutexIfLockedByThisInstance()
		{
			if (Mutex.HasLock)
			{
				Mutex.Unlock();
			}
		}

		ZGlobalMutex Mutex => mutex ?? (mutex = AsycudaManifestHeader.CreateMutex(HostBusinessEntity.PK));
		ZGlobalMutex mutex;
	}
}
