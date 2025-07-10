using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.Integration.DocumentEngine;

namespace Enterprise.DocumentEngine.Testing
{
	public class TemplateTestHelper : ITemplateTestHelper
	{
		readonly List<KeyValuePair<string, string>> workSheets = new List<KeyValuePair<string, string>>();

		public ZString DataContext { get; set; }

		public void SetDataContext(BusinessObject businessObject)
		{
			DataContext = string.Format(".{0}", businessObject.GetType().Name);
		}

		public void AddWorkSheet(string name, string contents)
		{
			workSheets.Add(new KeyValuePair<string, string>(name, contents.Trim()));
		}

		public void ClearWorkSheets()
		{
			workSheets.Clear();
		}

		public byte[] CreateTemplateBlob()
		{
			using (var stream = new MemoryStream())
			{
				DocumentEngineTestHelper.GenerateTemplateStream(stream, workSheets);
				return stream.CopyToByteArray();
			}
		}

		public StmTemplateBase CreateTemplate(BusinessObjectFactory factory, ZString name)
		{
			return CreateTemplate(factory, name, CreateTemplateBlob(), DataContext);
		}

		public static StmTemplateBase CreateTemplate(BusinessObjectFactory factory, ZString name, ZBlob template, ZString dataContext)
		{
			var result = factory.New<StmTemplateBase>();

			result.SO_Name = name;
			result.SO_Template = template;
			result.SO_DataContext = dataContext;

			return result;
		}
	}
}
