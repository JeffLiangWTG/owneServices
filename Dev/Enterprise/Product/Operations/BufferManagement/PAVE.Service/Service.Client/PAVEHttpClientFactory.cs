using System;
using Enterprise.Registry.Business;

namespace Enterprise.BufferManagement.Service.Client
{
	public class PAVEHttpClientFactory : IPAVEHttpClientFactory
	{
		internal static Uri BaseUri
		{
			get
			{
				var baseGlowUri = GlowRegistry.Instance.GlowServiceUri;

				if (string.IsNullOrEmpty(baseGlowUri) || baseGlowUri == "/")
				{
					return null;
				}

				return new Uri(baseGlowUri);
			}
		}

		public IPAVEHttpClient Create()
		{
			if (BaseUri == null)
			{
				return null;
			}

			return new PAVEHttpClient(BaseUri);
		}
	}
}
