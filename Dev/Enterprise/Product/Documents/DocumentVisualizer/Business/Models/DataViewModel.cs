using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Newtonsoft.Json.Linq;

namespace Enterprise.DocumentVisualizer.Models
{
	public sealed class DataViewModel : NonPersistentBusinessObject
	{
		public enum DataType
		{
			Messaging,
			Document,
			Overridden
		}

		public DataViewModel(IDocument document, DataType dataType, string userDefinedNamespace = "", string dataContext = "")
		{
			Argument.NotNull(document, nameof(document));
			this.document = document;
			this.data = document.Data;
			this.dataType = dataType;
			this.userDefinedNamespace = userDefinedNamespace;
			this.dataContext = dataContext;
		}

		readonly IDocument document;
		readonly IDynamicData data;
		readonly DataType dataType;
		readonly string userDefinedNamespace;
		readonly string dataContext;

		public ZString Text
		{
			get { return text; }
		}

		ZString text;

		public ZPropertyInfo TextInfo
		{
			get { return GetZPropertyInfo(nameof(Text)); }
		}

		public void UpdateText()
		{
			text = GetText();
			TextInfo.RefreshBinding();
		}

		string GetText()
		{
			var result = string.Empty;

			switch (dataType)
			{
				case DataType.Messaging:
					result = GetUXML(userDefinedNamespace);
					break;

				case DataType.Document:
					result = GetJSON();
					break;

				case DataType.Overridden:
					var xml = data.Serialize(d => d.IsOverriddenIncludingChildren);
					result = xml?.ToString();
					break;
			}

			return result ?? string.Empty;
		}

		string GetUXML(string ns)
		{
			return document.GetUXml(ns, dataContext)?.ToXmlString();
		}

		string GetJSON()
		{
			var map = data.ToMap();
			var json = map.ToJSON();

			var jsonObj = JObject.Parse(json);
			return jsonObj.ToString();
		}
	}
}
