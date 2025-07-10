using System.Text;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ExcelTemplates.Integration;

namespace Enterprise.ExcelTemplates.Testing
{
	sealed class TestStmTemplate : IStmTemplate
	{
		public ZString SO_Name
		{
			get { return "TestTemplate"; }
		}

		public ZString SO_ExcelTemplatePath
		{
			get { return "TestTemplate.xls"; }
		}

		public ZString SO_DataContext
		{
			get { return nameof(Constants.DataContext.Sailing); }
		}

		public ZBlob SO_Template
		{
			get { return Encoding.UTF8.GetBytes("test"); }
		}

		public ZBool SO_CannotEditDocumentData
		{
			get { return ZBool.False; }
		}

		public ZBool SO_IsClientSpecific
		{
			get { return ZBool.False; }
		}

		public ZBool SO_IsPasswordProtected
		{
			get { return ZBool.False; }
		}

		public ZBool SO_IsSystemDefined
		{
			get { return ZBool.False; }
		}

		public ZString SO_TemplateRestriction
		{
			get { return ZString.Empty; }
		}

		public ZBlob SO_UDFFieldCache
		{
			get { return ZBlob.Empty; }
		}
	}
}
