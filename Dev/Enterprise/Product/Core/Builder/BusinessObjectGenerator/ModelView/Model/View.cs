using System.Collections.Generic;
using System.Xml.Serialization;

namespace Enterprise.BusinessObjectGenerator.ModelView
{
	public class View
	{
		public List<AddInfo> AddInfos { get; set; }

		string searchCondition;

		public string SearchCondition
		{
			get { return searchCondition; }
			set { searchCondition = value?.Trim(); }
		}

		[XmlAttribute(AttributeName = "Table")]
		public string Table { get; set; }

		[XmlAttribute(AttributeName = "DevelopmentOnly")]
		public bool DevelopmentOnly { get; set; }

		[XmlAttribute(AttributeName = "AddInfoColumn")]
		public string AddInfoColumn { get; set; }

		[XmlAttribute(AttributeName = "NAddInfoColumn")]
		public string NAddInfoColumn { get; set; }

		[XmlAttribute(AttributeName = "ParentView")]
		public string ParentView { get; set; }

		public ModelViewContext ParentContext { get; set; }
	}
}
