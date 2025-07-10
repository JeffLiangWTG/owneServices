using System;
using CargoWise.Common.Testing;

namespace Enterprise.DbUpgrader.DataProxy
{
	[SuppressClassNamesAreUniqueAcrossAssembliesMessage]
	class DocumentsVersion
	{
		public static int VersionNumber
		{
			get { return (int)Type.GetType("Enterprise.DbUpgrader.Data.DocumentsVersion, ExcelTemplates").GetField("VersionNumber").GetValue(null); }
		}
	}
}
