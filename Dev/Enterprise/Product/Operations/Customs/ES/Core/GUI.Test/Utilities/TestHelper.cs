using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing
{
	public static class TestHelper
	{
		public static void CheckFactoryHasNoPendingChanges(ZString messagePrefix, BusinessObjectFactory factory)
		{
			var mainFactoryChangeSet = factory.GetChanges();

			bool mainFactoryHasChanges =
				mainFactoryChangeSet.GetChangedObjects().Any()
				|| mainFactoryChangeSet.GetAddedObjects().Any();

			Assertion.AssertEquals(messagePrefix + " [All changes should be made in the sending factory] Does Main Factory have changes?", false, mainFactoryHasChanges);
		}

		public static void AssertColumnStyle<T>(ZString message, ZGrid grid, ZString columnName) where T : ZGridColumnInfo
		{
			var column = grid.ColumnStyles.Cast<ZGridColumnInfo>().SingleOrDefault(x => x.ColumnName == columnName);

			Assertion.AssertNotNull(message, column);
			Assertion.AssertType<T>(column);
		}

		public static void AssertColumnStyleWithCaption<T>(ZString caption, ZGrid grid, ZString columnName) where T : ZGridColumnInfo
		{
			var column = grid.ColumnStyles.Cast<ZGridColumnInfo>().SingleOrDefault(x => x.ColumnName == columnName);

			Assertion.AssertNotNull(caption, column);
			Assertion.AssertType<T>(column);
			Assertion.AssertEquals(columnName + " column caption is correct", caption, column.CaptionResourceString.Caption);
		}

		public static void AssertColumnStyleWithCaptionWithVisibility<T>(ZString caption, ZGrid grid, ZString columnName, ZBool isVisible) where T : ZGridColumnInfo
		{
			var column = grid.ColumnStyles.Cast<ZGridColumnInfo>().SingleOrDefault(x => x.ColumnName == columnName);

			Assertion.AssertNotNull(caption, column);
			Assertion.AssertType<T>(column);
			Assertion.AssertEquals(columnName + " column caption is correct", caption, column.CaptionResourceString.Caption);
			Assertion.AssertEquals(columnName + " column is visible", isVisible, column.IsVisible);
		}
	}
}
