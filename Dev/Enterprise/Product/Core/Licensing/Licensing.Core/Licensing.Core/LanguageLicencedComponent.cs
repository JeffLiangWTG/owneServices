using System;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Licensing
{
	class LanguageLicencedComponent : ILicensedComponent
	{
		public static LanguageLicencedComponent Instance { get; private set; }

		static LanguageLicencedComponent()
		{
			Instance = new LanguageLicencedComponent();
		}

		public IDisposable LicensedComponentManager
		{
			get { return new LicensedComponentManager(this); }
		}
	}
}
