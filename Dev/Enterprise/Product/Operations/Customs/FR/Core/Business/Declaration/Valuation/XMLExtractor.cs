using System;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public static class XMLExtractor
	{
		/// <summary>
		/// Reads an <see cref="IncoTermsConfiguration"/> definition from the given resource.
		/// </summary>
		/// <param name="resourceName">The name of the resource.</param>
		/// <exception cref="InvalidOperationException">The given resource exists but it is not an <see cref="IncoTermsConfiguration"/> definition.</exception>
		/// <returns>The parsed <see cref="IncoTermsConfiguration"/> definition if it exists; otherwise, null.</returns>
		public static IncoTermsConfiguration GetIncoTermsConfiguration(string resourceName)
		{
			using (var stream = typeof(XMLExtractor).Assembly.GetManifestResourceStream(resourceName))
			{
				if (stream == null)
				{
					return null;
				}

				var serializer = ZXmlSerializer.New(typeof(IncoTermsConfiguration));
				return (IncoTermsConfiguration)serializer.Deserialize(stream);
			}
		}

		public static FranceFreightCalculation GetFreightCalculation(string resourceName)
		{
			using (var stream = typeof(XMLExtractor).Assembly.GetManifestResourceStream(resourceName))
			{
				if (stream == null)
				{
					return null;
				}

				var serializer = ZXmlSerializer.New(typeof(FranceFreightCalculation));
				return (FranceFreightCalculation)serializer.Deserialize(stream);
			}
		}
	}
}
