using System;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	sealed class ReportColumnSettingsRetrieverTest : TransactionedTestCase
	{
		public void TestSetAndGet()
		{
			Guid reportPk1 = Guid.NewGuid();
			Guid reportPk2 = Guid.NewGuid();
			Guid reportPk3 = Guid.NewGuid();

			Guid clientPk1 = Guid.NewGuid();
			Guid clientPk2 = Guid.NewGuid();

			Retriever.Set("", reportPk1, clientPk1, "r1c1");
			Retriever.Set("", reportPk2, clientPk1, "r2c1");
			Retriever.Set("", reportPk1, clientPk2, "r1c2");
			Retriever.Set("", reportPk2, clientPk2, "r2c2");
			Retriever.Set("", reportPk1, clientPk1, "Code1", "r1c1Code1");
			Retriever.Set("", reportPk1, clientPk1, "Code2", "r1c1Code2");
			Retriever.Set("", reportPk1, clientPk2, "Code3", "r1c2Code3");

			AssertEquals("Get(reportPk1, clientPk1)", "r1c1", Retriever.Get("", reportPk1, clientPk1));
			AssertEquals("Get(reportPk1, clientPk2)", "r1c2", Retriever.Get("", reportPk1, clientPk2));
			AssertEquals("Get(reportPk2, clientPk1)", "r2c1", Retriever.Get("", reportPk2, clientPk1));
			AssertEquals("Get(reportPk2, clientPk2)", "r2c2", Retriever.Get("", reportPk2, clientPk2));
			AssertEquals("Get(reportPk1, clientPk1, Code1)", "r1c1Code1", Retriever.Get("", reportPk1, clientPk1, "Code1"));
			AssertEquals("Get(reportPk1, clientPk1, Code2)", "r1c1Code2", Retriever.Get("", reportPk1, clientPk1, "Code2"));
			AssertEquals("Get(reportPk1, clientPk2, Code3)", "r1c2Code3", Retriever.Get("", reportPk1, clientPk2, "Code3"));
			AssertEquals("Get(reportPk3, clientPk1, Code1)", "", Retriever.Get("", reportPk3, clientPk1, "Code1"));
			AssertEquals("Get(reportPk3, clientPk1, Code2)", "", Retriever.Get("", reportPk3, clientPk1, "Code2"));
			AssertEquals("Get(reportPk3, clientPk2, Code3)", "", Retriever.Get("", reportPk3, clientPk2, "Code3"));
			AssertEquals("Get(reportPk3, clientPk1)", "", Retriever.Get("", reportPk3, clientPk1));
			AssertEquals("Get(reportPk3, clientPk2)", "", Retriever.Get("", reportPk3, clientPk2));

			Retriever.Set("n1", reportPk1, "r1n1");
			Retriever.Set("n1", reportPk2, "r2n1");
			Retriever.Set("n2", reportPk1, "r1n2");
			Retriever.Set("n2", reportPk2, "r2n2");
			Retriever.Set("n3", reportPk1, "r1n3");
			Retriever.Set("n3", reportPk2, "r2n3");

			AssertEquals("Get(reportPk1, \"n1\")", "r1n1", Retriever.Get("n1", reportPk1));
			AssertEquals("Get(reportPk1, \"n2\")", "r1n2", Retriever.Get("n2", reportPk1));
			AssertEquals("Get(reportPk1, \"n3\")", "r1n3", Retriever.Get("n3", reportPk1));
			AssertEquals("Get(reportPk2, \"n1\")", "r2n1", Retriever.Get("n1", reportPk2));
			AssertEquals("Get(reportPk2, \"n2\")", "r2n2", Retriever.Get("n2", reportPk2));
			AssertEquals("Get(reportPk2, \"n3\")", "r2n3", Retriever.Get("n3", reportPk2));
			AssertEquals("Get(reportPk3, \"n1\")", "", Retriever.Get("n1", reportPk3));
			AssertEquals("Get(reportPk3, \"n2\")", "", Retriever.Get("n2", reportPk3));
			AssertEquals("Get(reportPk3, \"n3\")", "", Retriever.Get("n3", reportPk3));
		}

		public void TestSetTwice()
		{
			Guid reportPK = Guid.NewGuid();
			Guid clientPK = Guid.NewGuid();

			Retriever.Set("n", reportPK, "n1");
			Retriever.Set("", reportPK, clientPK, "c1");
			AssertEquals("Get(reportPK, \"n\")", "n1", Retriever.Get("n", reportPK));
			AssertEquals("Get(reportPK, clientPK)", "c1", Retriever.Get("", reportPK, clientPK));

			Retriever.Set("n", reportPK, "v2");
			Retriever.Set("", reportPK, clientPK, "c2");
			AssertEquals("Get(reportPK, \"n\")", "v2", Retriever.Get("n", reportPK));
			AssertEquals("Get(reportPK, clientPK)", "c2", Retriever.Get("", reportPK, clientPK));
		}

		public void TestDelete()
		{
			Guid reportPK = Guid.NewGuid();
			Guid clientPk1 = Guid.NewGuid();
			Guid clientPk2 = Guid.NewGuid();

			Retriever.Delete("n1", reportPK);
			Retriever.Delete("", reportPK, clientPk1);

			Retriever.Set("n1", reportPK, "n1");
			Retriever.Set("n2", reportPK, "n2");
			Retriever.Set("", reportPK, clientPk1, "c1");
			Retriever.Set("", reportPK, clientPk2, "c2");

			Retriever.Delete("n1", reportPK);
			Retriever.Delete("n3", reportPK);
			Retriever.Delete("", reportPK, clientPk2);
			Retriever.Delete("", reportPK, Guid.NewGuid());
			Retriever.Delete("n1", Guid.NewGuid());
			Retriever.Delete("", Guid.NewGuid(), clientPk1);

			AssertEquals("Get(reportPK, \"n1\")", "", Retriever.Get("n1", reportPK));
			AssertEquals("Get(reportPK, \"n2\")", "n2", Retriever.Get("n2", reportPK));
			AssertEquals("Get(reportPK, clientPk1)", "c1", Retriever.Get("", reportPK, clientPk1));
			AssertEquals("Get(reportPK, clientPk2)", "", Retriever.Get("", reportPK, clientPk2));

			Retriever.Delete("n2", reportPK);
			AssertEquals("Get(reportPK, \"n2\")", "", Retriever.Get("n2", reportPK));
		}

		#region Implementation

		ReportColumnSettingsRetriever Retriever
		{
			get
			{
				if (retriever == null)
				{
					retriever = new ReportColumnSettingsRetriever();
				}
				return retriever;
			}
		}
		ReportColumnSettingsRetriever retriever;
		#endregion
	}
}
