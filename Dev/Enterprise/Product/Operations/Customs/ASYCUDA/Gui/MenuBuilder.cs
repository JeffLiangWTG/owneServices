using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public abstract class MenuBuilder
	{
		protected MenuBuilder(AsycudaManifestHeader header, ZForm mainForm)
		{
			this.Header = Argument.NotNull(header, nameof(header));
			this.mainForm = Argument.NotNull(mainForm, nameof(mainForm));
		}

		protected ZBool IsValidForMessage()
		{
			var header = Header;
			header?.Validation.ValidateAMA_RN_NKCountry();
			return header != null && !header.AMA_RN_NKCountryInfo.HasMessageErrors();
		}

		public ZMenuItem GetInvalidMessageMenuItem()
		{
			var caption = Enterprise.Customs.ASYCUDA.Gui.ResString.GetMultilingualString("AsycudaMenu|NoValidManifest", "Send &Manifest");
			var noSentMenuItem = new ZMenuItem(caption);

			noSentMenuItem.Click += delegate
			{
				var header = Header;
				header?.Validation.ValidateAMA_RN_NKCountry();

				var errors = header?.AMA_RN_NKCountryInfo.GetMessageErrors()
					.Select(c => c.Message)
					.Distinct()
					.ToArray() ?? Array.Empty<string>();

				if (errors.Any())
				{
					var message = ZString.Format(
						"There are message errors that need to be corrected before this Manifest Message can be sent.{0}{0}{1}",
						System.Environment.NewLine,
						string.Join(System.Environment.NewLine, errors));

					Globals.Message.ShowError(message, caption);
				}
			};

			return noSentMenuItem;
		}

		public readonly AsycudaManifestHeader Header;
		public readonly ZForm mainForm;

		public abstract ResourceString MenuCaption { get; }

		public abstract ZMenuItem[] BuildMenu();
	}
}
