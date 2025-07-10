using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.NumberFountain.Internal;

namespace Enterprise.NumberFountain.Testing
{
	internal class NonFormattedNumberFountainWithMainConnectionTest : NonFormattedNumberFountainTest
	{
		protected override DbConnection TestConnection()
		{
			return Db.Connection;
		}
	}

	internal class NonFormattedNumberFountainWithAnotherConnectionTest : NonFormattedNumberFountainTest
	{
		protected override DbConnection TestConnection()
		{
			var conn = Db.NewExtraConnectionToMainDb();
			connections.Add(conn);
			return conn;
		}

		readonly List<DbConnection> connections = new List<DbConnection>();

		protected override void TearDown()
		{
			base.TearDown();
			foreach (var conn in connections)
			{
				conn.Dispose();
			}
		}
	}

	internal abstract class NonFormattedNumberFountainTest : NumberFountainTestBase
	{
		protected override INumberFountain GetFountain(string name, Guid ownerPk, bool rollover, int minValue, int maxValue)
		{
			return new NonFormattedNumberFountain(name, ownerPk, rollover, minValue, maxValue);
		}

		protected override void Constructor(string name, Guid ownerPk)
		{
			const bool rollover = true;
			const int minValue = 1;
			const int maxValue = 9;
			var fountain = (IFountain)GetFountain(name, ownerPk, rollover, minValue, maxValue);
			TestFountainConstructor(fountain, name, ownerPk, rollover, minValue, maxValue);
		}

		protected override string IntToStringForGetNext(int i)
		{
			return i.ToString();
		}
	}
}
