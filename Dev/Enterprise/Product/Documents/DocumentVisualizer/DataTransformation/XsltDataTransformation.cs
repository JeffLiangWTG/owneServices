using System.Xml.Linq;
using System.Xml.Xsl;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.DataTransformation
{
	abstract class XsltDataTransformation : IDataTransformation
	{
		public abstract string Description { get; }
		public abstract string DataStoreName { get; }
		protected abstract string GetXslt();

		public XDocument Run(XDocument input, INotificationsHandler handler)
		{
			var xslt = XDocument.Parse(GetXslt());
			var result = new XDocument();

			using (var xsltReader = xslt.CreateReader())
			using (var inputReader = input.CreateReader())
			using (var resultWriter = result.CreateWriter())
			{
				var transform = new XslCompiledTransform();
				transform.Load(xsltReader);
				transform.Transform(inputReader, null, resultWriter);
			}

			OnAfterRun(input, xslt, result, handler);

			return result;
		}

		protected virtual void OnAfterRun(XDocument input, XDocument xslt, XDocument result, INotificationsHandler handler)
		{
		}
	}
}