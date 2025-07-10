using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentEngine
{
	/// <summary>
	/// Summary description for StmMenuEDocsBase.
	/// </summary>
	public class DocumentStmMenuEDocs : StmMenuEDocs, IRootTypeProvider
	{
		public DocumentStmMenuEDocs(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region SX_DocType

		[CargoWise.ComponentModel.MaxLength(50)]
		public ZString SX_DocType
		{
			get { return (DocType != null) ? DocType.RT_DocType : ZString.Empty; }
		}

		public ZPropertyInfo SX_DocTypeInfo
		{
			get { return GetZPropertyInfo(nameof(SX_DocType)); }
		}

		#endregion

		#region SX_Description

		[CargoWise.ComponentModel.MaxLength(50)]
		public ZString SX_Description
		{
			get { return (DocType != null) ? DocType.RT_DescMultilingual : ZString.Empty; }
		}

		public ZPropertyInfo SX_DescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(SX_Description)); }
		}

		#endregion

		protected override StmMenuEDocsValidation GetNewValidation()
		{
			return new DocumentStmMenuEDocsValidation(this);
		}

		#region IRootTypeProvider Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Interface member of IRootTypeProvider")]
		public Type[] RootTypes => new[] { typeof(IeDoc) };

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Interface member of IRootTypeProvider")]
		public BusinessObject[] Roots => Array.Empty<BusinessObject>();

		#endregion
	}
}
