using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public static class UniversalObjectFactoryExtensions
	{
		/// <summary>
		/// The Factory does not handle Deletes properly when a mix of Data Rows and BizOs are deleted, so until
		/// Universal does not require BizOs, we will load the BizO and manually set HasChanges.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1004")]
		public static void DeleteRowAndSetHasChanges<T>(this UniversalObjectFactory factory, IColumnIndexer columnIndexer, SchemaPKColumn pkColumn)
			where T : BusinessObject
		{
			var pk = columnIndexer.GetValue(pkColumn);
			columnIndexer.DeleteRowAndSetHasChanges(factory.Load<T>(pk));
		}

		/// <summary>
		/// The Factory does not handle Deletes properly when a mix of Data Rows and BizOs are deleted, so until
		/// Universal does not require BizOs, we will manually set HasChanges.
		/// </summary>
		public static void DeleteRowAndSetHasChanges<T>(this IColumnIndexer columnIndexer, T bizO)
			where T : BusinessObject
		{
			bizO.HasChanges = true;
			columnIndexer.Delete();
		}
	}
}