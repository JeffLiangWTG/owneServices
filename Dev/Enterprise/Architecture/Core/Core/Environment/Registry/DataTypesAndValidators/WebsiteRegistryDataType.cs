using System;
using Enterprise.Integration;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public class WebsiteRegistryDataType : StringRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (!string.IsNullOrEmpty(proposedValue))
			{
				if (!Uri.TryCreate(proposedValue, UriKind.Absolute, out var website))
				{
					throw new RegistryValidationException(Res.GetString("4c55231d-1f70-4305-a1f8-215cb1985c65", "Web address is in an invalid format"));
				}

				if (website.Scheme != Uri.UriSchemeHttp && website.Scheme != Uri.UriSchemeHttps)
				{
					throw new RegistryValidationException(Res.GetString("e13f3881-a43d-4d83-b2cf-35e7773003c6", "{0} protocol is not supported. Must be HTTP or HTTPS", website.Scheme));
				}
			}
		}
	}
}
