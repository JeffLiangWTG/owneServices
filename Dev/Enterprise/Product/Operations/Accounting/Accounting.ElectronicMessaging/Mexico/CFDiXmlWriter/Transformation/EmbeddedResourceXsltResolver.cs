using System;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico
{
	public class EmbeddedResourceXsltResolver : XmlResolver
	{
		public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
		{
			var executingAssembly = Assembly.GetExecutingAssembly();
			foreach (var resourceName in executingAssembly.GetManifestResourceNames())
			{
				if (resourceName.EndsWith("." + System.IO.Path.GetFileName(absoluteUri.LocalPath)))
				{
					var xslt = executingAssembly.GetManifestResourceStream(resourceName);
					return XmlReader.Create(new StreamReader(xslt), null, absoluteUri.LocalPath);
				}
			}
			throw new FileNotFoundException("Did not find xslt as embedded resource.", absoluteUri.LocalPath);
		}
	}
}
