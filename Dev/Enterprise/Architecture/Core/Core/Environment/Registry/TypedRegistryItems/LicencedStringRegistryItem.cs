using System;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public interface ILicencedRegistryItem
	{
		bool IsLicensed { get; }
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public class LicencedStringRegistryItem : StringRegistryItem, ILicencedRegistryItem
	{
		public LicencedStringRegistryItem(Func<bool> hasLicence, string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(hasLicence, name, category, caption, hint, storage, RegistryOptions.Default)
		{
		}

		public LicencedStringRegistryItem(Func<bool> hasLicence, string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(name, category, caption, hint, storage, options)
		{
			this.hasLicence = Argument.NotNull(hasLicence, "hasLicence");
		}

		public bool IsLicensed { get { return hasLicence(); } }
		readonly Func<bool> hasLicence;
	}
}
