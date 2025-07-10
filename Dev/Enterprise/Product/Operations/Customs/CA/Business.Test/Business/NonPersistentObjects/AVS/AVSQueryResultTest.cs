using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(AVSQueryResult))]
	sealed class AVSQueryResultTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var note = Factory.New<StmNote>();
			note.ST_NoteText = "Incorrect requirement and/or version";
			var queryResult = new AVSQueryResult(note);
			AssertEquals("QueryTime", ZDateTime.Empty, queryResult.QueryTime);
			AssertEquals("QueryResult", "Incorrect requirement and/or version", queryResult.QueryResult);

			note.ST_NoteText = "2016-02-14T21:52:29|Incorrect requirement and/or version";
			queryResult = new AVSQueryResult(note);
			AssertEquals("QueryTime", new ZDateTime(2016, 2, 14, 21, 52, 29), queryResult.QueryTime);
			AssertEquals("QueryResult", "Incorrect requirement and/or version", queryResult.QueryResult);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AVSQueryResult(Factory.New<StmNote>());
		}

		#endregion
	}
}
