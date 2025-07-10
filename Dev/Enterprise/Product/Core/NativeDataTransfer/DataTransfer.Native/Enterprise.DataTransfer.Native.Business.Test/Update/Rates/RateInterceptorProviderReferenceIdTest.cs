using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Adapter.ImportServices;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Logging;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update.Rates
{
	public class RateInterceptorProviderReferenceIdTest : TransactionedTestCase
	{
		readonly struct RateTestData(Dictionary<string, string> replacingValues)
		{
			public static RateTestData NewWithValidTestData()
			{
				var replacingValues = new Dictionary<string, string>
				{
					// Header
					{ "{RATETYPE}", "<RateType>SAL</RateType>" },
					{ "{RATELEVEL}", "0" },
					// ====================
					// RateEntry
					{ "{RATEENTRY_ACTION}", "MERGE" },
					{ "{RATEENTRY_PK}", ToPkTag("78757535-af18-4c06-95f5-b94df4e66295") },
					{ "{RATEENTRY_PROVIDER_REFERENCE_ID}", "ENTRY_ID" },
					{ "{RATE_START_DATE}", "2016-01-01T00:00:00" },
					{ "{RATE_END_DATE}", string.Empty },
					{ "{ORIGIN}", "AUSYD" },
					// RateLine
					{ "{RATELINE_ACTION}", "MERGE" },
					{ "{RATELINE_PK}", ToPkTag("645806d9-8481-4c1a-ab53-f8ff9e4b7f63") },
					{ "{ROUNDING}", "DEF" },
					{ "{RATELINE_PROVIDER_REFERENCE_ID}", "LINE_ID" },
				};

				return new RateTestData(replacingValues);
			}

			/// <summary>
			///		Generate XML from a template with the replacements defined in ReplacingValues dictionary.
			/// </summary>
			public (string originalTemplate, string outputXML) GenerateXmlFromTemplateFile(string templateFile)
			{
				if (ReplacingValues is not { Count: > 0 })
				{
					throw new InvalidOperationException("ReplacingValues must be set before calling this method.");
				}

				var data = new StreamReader(GetType().Assembly.GetManifestResourceStream(templateFile));
				var template = data.ReadToEnd();

				return (template, GenerateXmlFromTemplate(template));
			}

			public string GenerateXmlFromTemplate(string templateString)
			{
				var outputXml = new StringBuilder(templateString);

				foreach (var pair in ReplacingValues)
				{
					outputXml.Replace(pair.Key, pair.Value);
				}

				return outputXml.ToString();
			}

			public void Replace(string templateString, string templateValue)
			{
				ReplacingValues[templateString] = templateValue;
			}

			Dictionary<string, string> ReplacingValues { get; } = replacingValues;
		}

		enum DbRateSetupFlags
		{
			NoMatching,
			// the belows imply that there is a matching rate in the DB
			NoOverlappedDate,
			FullyOverlappedDate,
			PartiallyOverlappedDate,
		}

		enum PkSetupFlags
		{
			NoPkInXml,
			// the belows imply HasPkInXml
			RateHasDifferentPkThanXmlOne,
			RateHasSamePkAsTheXmlOne,
		}

		enum ProviderReferenceSetupFlags
		{
			NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			// the belows imply HasProviderReferenceIdInXml
			HasRateWithSameProviderReferenceId,
			HasRateWithDifferentProviderReferenceId,
		}

		struct TestCase
		{
			public DbRateSetupFlags DbRateSetup { get; set; }
			public PkSetupFlags PkSetup { get; set; }
			public ProviderReferenceSetupFlags ProviderReferenceSetup { get; set; }
			public string Action { get; set; }
			public string AssertionMessage { get; set; }
			public string[] ExpectedRates { get; set; }
			public string[] ExpectedLogStrings { get; set; }
			public bool ExpectNotHavingError { get; set; }
		}

		#region INSERT action

		public void TestImport_Insert_NoPkInXml_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "INSERT: when there is no matching rate, should insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Insert_NoPkInXml_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "INSERT: when there is no overlapped rate, should insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Insert_NoPkInXml_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "INSERT: when there is a partially overlapped rate, should expire it and insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,31-Jan-16,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Insert_NoPkInXml_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "INSERT: when there is a fully overlapped rate, should fail with error raised.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: Incoming RateEntry date range conflicts with existing RateEntry.",
				"Existing RateEntry date range: 01-Jan-16 00:00:00 - Empty date.",
				"Incoming RateEntry date range 01-Jan-16 00:00:00 - Empty date",
			}
		});

		public void TestImport_Insert_NoPkInXml_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "INSERT: when there is no matching rate, should insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Insert_NoPkInXml_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "INSERT: when there is no overlapped rate, should insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Insert_NoPkInXml_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "INSERT: when there is a partially overlapped rate, should expire it and insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,31-Jan-16,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Insert_NoPkInXml_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "INSERT: when there is a fully overlapped rate, should fail with error raised.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: Incoming RateEntry date range conflicts with existing RateEntry.",
				"Existing RateEntry date range: 01-Jan-16 00:00:00 - Empty date.",
				"Incoming RateEntry date range 01-Jan-16 00:00:00 - Empty date",
			}
		});

		public void TestImport_Insert_NoPkInXml_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "INSERT: when there is no matching rate, should insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Insert_NoPkInXml_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "INSERT: when there is no overlapped rate, should insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Insert_NoPkInXml_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "INSERT: when there is a partially overlapped rate, should expire it and insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,31-Jan-16,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Insert_NoPkInXml_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "INSERT: when there is a fully overlapped rate, should fail with error raised.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: Incoming RateEntry date range conflicts with existing RateEntry.",
				"Existing RateEntry date range: 01-Jan-16 00:00:00 - Empty date.",
				"Incoming RateEntry date range 01-Jan-16 00:00:00 - Empty date",
			}
		});

		public void TestImport_Insert_NoPkInXml_HasRateWithDifferentProviderReferenceId_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "INSERT: when there is no matching rate, should insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Insert_NoPkInXml_HasRateWithDifferentProviderReferenceId_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "INSERT: when there is no overlapped rate, should insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Insert_NoPkInXml_HasRateWithDifferentProviderReferenceId_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "INSERT: when there is a partially overlapped rate, should expire it and insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,31-Jan-16,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Insert_NoPkInXml_HasRateWithDifferentProviderReferenceId_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "INSERT: when there is a fully overlapped rate, should fail with error raised.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: Incoming RateEntry date range conflicts with existing RateEntry.",
				"Existing RateEntry date range: 01-Jan-16 00:00:00 - Empty date.",
				"Incoming RateEntry date range 01-Jan-16 00:00:00 - Empty date",
			}
		});

		public void TestImport_Insert_NoPkInXml_HasRateWithSameProviderReferenceId_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "INSERT: when there is no matching rate, should not insert the new one because of a constraint violation.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: Error from Data layer: TableName=RateEntry",
				"InnerException Message = Cannot insert duplicate key row in object 'dbo.RateEntry' with unique index 'NR_UX__TI_TH_TI_ProviderReferenceID'. The duplicate key value is",
			},
		});

		public void TestImport_Insert_NoPkInXml_HasRateWithSameProviderReferenceId_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "INSERT: when there is no overlapped rate, should not insert the new one because of a constraint violation.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: Error from Data layer: TableName=RateEntry",
				"InnerException Message = Cannot insert duplicate key row in object 'dbo.RateEntry' with unique index 'NR_UX__TI_TH_TI_ProviderReferenceID'. The duplicate key value is",
			},
		});

		public void TestImport_Insert_NoPkInXml_HasRateWithSameProviderReferenceId_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "INSERT: when there is a partially overlapped rate, should expire it but the new rate should not be inserted because of a constraint violation.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,31-Jan-16,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Existing RateEntry was expired by incoming RateEntry",
				"Test error: Error from Data layer: TableName=RateEntry",
				"InnerException Message = Cannot insert duplicate key row in object 'dbo.RateEntry' with unique index 'NR_UX__TI_TH_TI_ProviderReferenceID'. The duplicate key value is",
			},
		});

		public void TestImport_Insert_NoPkInXml_HasRateWithSameProviderReferenceId_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "INSERT: when there is a fully overlapped rate, should fail with error raised.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: Incoming RateEntry date range conflicts with existing RateEntry.",
				"Existing RateEntry date range: 01-Jan-16 00:00:00 - Empty date.",
				"Incoming RateEntry date range 01-Jan-16 00:00:00 - Empty date",
			}
		});

		///

		public void TestImport_Insert_DifferentPk_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "INSERT: should insert new rate because cannot find rate rate from data in XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Insert_DifferentPk_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "INSERT: should insert new rate because cannot find rate rate from data in XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Insert_DifferentPk_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "INSERT: existing rate should be expired and the new rate should be inserted.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,31-Jan-16,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Insert_DifferentPk_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "INSERT: should fail with a date range conflict message.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: Incoming RateEntry date range conflicts with existing RateEntry.",
				"Existing RateEntry date range: 01-Jan-16 00:00:00 - Empty date.",
				"Incoming RateEntry date range 01-Jan-16 00:00:00 - Empty date",
			}
		});

		public void TestImport_Insert_DifferentPk_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "INSERT: the new rate should be inserted.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Insert_DifferentPk_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "INSERT: should insert new rate because cannot find rate rate from data in XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Insert_DifferentPk_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "INSERT: existing rate should be expired and the new rate should be inserted.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,31-Jan-16,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Insert_DifferentPk_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "INSERT: should fail with a date range conflict message.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: Incoming RateEntry date range conflicts with existing RateEntry.",
				"Existing RateEntry date range: 01-Jan-16 00:00:00 - Empty date.",
				"Incoming RateEntry date range 01-Jan-16 00:00:00 - Empty date",
			}
		});

		public void TestImport_Insert_DifferentPk_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "INSERT: when there is no matching rate, should insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Insert_DifferentPk_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "INSERT: when there is no overlapped rate, should insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Insert_DifferentPk_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "INSERT: when there is a partially overlapped rate, should expire it and insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,31-Jan-16,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Insert_DifferentPk_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "INSERT: when there is a fully overlapped rate, should fail with error raised.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: Incoming RateEntry date range conflicts with existing RateEntry.",
				"Existing RateEntry date range: 01-Jan-16 00:00:00 - Empty date.",
				"Incoming RateEntry date range 01-Jan-16 00:00:00 - Empty date",
			}
		});

		public void TestImport_Insert_DifferentPk_HasRateWithDifferentProviderReferenceId_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "INSERT: when there is no matching rate, should insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Insert_DifferentPk_HasRateWithDifferentProviderReferenceId_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "INSERT: when there is no overlapped rate, should insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Insert_DifferentPk_HasRateWithDifferentProviderReferenceId_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "INSERT: when there is a partially overlapped rate, should expire it and insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,31-Jan-16,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Insert_DifferentPk_HasRateWithDifferentProviderReferenceId_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "INSERT: when there is a fully overlapped rate, should fail with error raised.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: Incoming RateEntry date range conflicts with existing RateEntry.",
				"Existing RateEntry date range: 01-Jan-16 00:00:00 - Empty date.",
				"Incoming RateEntry date range 01-Jan-16 00:00:00 - Empty date",
			}
		});

		public void TestImport_Insert_DifferentPk_HasRateWithSameProviderReferenceId_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "INSERT: when there is no matching rate, should not insert the new one because of a constraint violation.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: Error from Data layer: TableName=RateEntry",
				"InnerException Message = Cannot insert duplicate key row in object 'dbo.RateEntry' with unique index 'NR_UX__TI_TH_TI_ProviderReferenceID'. The duplicate key value is",
			},
		});

		public void TestImport_Insert_DifferentPk_HasRateWithSameProviderReferenceId_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "INSERT: when there is no overlapped rate, should not insert the new one because of a constraint violation.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: Error from Data layer: TableName=RateEntry",
				"InnerException Message = Cannot insert duplicate key row in object 'dbo.RateEntry' with unique index 'NR_UX__TI_TH_TI_ProviderReferenceID'. The duplicate key value is",
			},
		});

		public void TestImport_Insert_DifferentPk_HasRateWithSameProviderReferenceId_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "INSERT: when there is partially overlapped rate, should expire the rate but should not insert the new one because of a constraint violation.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,31-Jan-16,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: Error from Data layer: TableName=RateEntry",
				"InnerException Message = Cannot insert duplicate key row in object 'dbo.RateEntry' with unique index 'NR_UX__TI_TH_TI_ProviderReferenceID'. The duplicate key value is",
			},
		});

		public void TestImport_Insert_DifferentPk_HasRateWithSameProviderReferenceId_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "INSERT: when there is a fully overlapped rate, should fail with error raised.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: Incoming RateEntry date range conflicts with existing RateEntry.",
				"Existing RateEntry date range: 01-Jan-16 00:00:00 - Empty date.",
				"Incoming RateEntry date range 01-Jan-16 00:00:00 - Empty date",
			}
		});

		///

		public void TestImport_Insert_SamePk_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "INSERT: should insert new rate because cannot find rate rate from data in XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Insert_SamePk_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "INSERT: the new rate should be inserted.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Insert_SamePk_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "INSERT: the new rate should be inserted.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,31-Jan-16,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Insert_SamePk_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "INSERT: when there is a fully overlapped rate, should fail with a date range error.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: Incoming RateEntry date range conflicts with existing RateEntry.",
				"Existing RateEntry date range: 01-Jan-16 00:00:00 - Empty date.",
				"Incoming RateEntry date range 01-Jan-16 00:00:00 - Empty date",
			}
		});

		public void TestImport_Insert_SamePk_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "INSERT: the new rate should be inserted.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Insert_SamePk_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "INSERT: the new rate should be inserted.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Insert_SamePk_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "INSERT: should expire the existing rate and insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,31-Jan-16,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Insert_SamePk_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "INSERT: when there is a fully overlapped rate, should fail with a date range error.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: Incoming RateEntry date range conflicts with existing RateEntry.",
				"Existing RateEntry date range: 01-Jan-16 00:00:00 - Empty date.",
				"Incoming RateEntry date range 01-Jan-16 00:00:00 - Empty date",
			}
		});

		public void TestImport_Insert_SamePk_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "INSERT: when there is no matching rate, should insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Insert_SamePk_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "INSERT: when there is no overlapped rate, should insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Insert_SamePk_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "INSERT: when there is a partially overlapped rate, should expire it and insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,31-Jan-16,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Insert_SamePk_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "INSERT: when there is a fully overlapped rate, should fail with error raised.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: Incoming RateEntry date range conflicts with existing RateEntry.",
				"Existing RateEntry date range: 01-Jan-16 00:00:00 - Empty date.",
				"Incoming RateEntry date range 01-Jan-16 00:00:00 - Empty date",
			}
		});

		public void TestImport_Insert_SamePk_HasRateWithDifferentProviderReferenceId_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "INSERT: when there is no matching rate, should insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Insert_SamePk_HasRateWithDifferentProviderReferenceId_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "INSERT: when there is no overlapped rate, should insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Insert_SamePk_HasRateWithDifferentProviderReferenceId_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "INSERT: when there is a partially overlapped rate, should expire it and insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,31-Jan-16,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Insert_SamePk_HasRateWithDifferentProviderReferenceId_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "INSERT: when there is a fully overlapped rate, should fail with error raised.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: Incoming RateEntry date range conflicts with existing RateEntry.",
				"Existing RateEntry date range: 01-Jan-16 00:00:00 - Empty date.",
				"Incoming RateEntry date range 01-Jan-16 00:00:00 - Empty date",
			}
		});

		public void TestImport_Insert_SamePk_HasRateWithSameProviderReferenceId_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "INSERT: when there is no matching rate, should not insert the new one because of a constraint violation.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: Error from Data layer: TableName=RateEntry",
				"InnerException Message = Cannot insert duplicate key row in object 'dbo.RateEntry' with unique index 'NR_UX__TI_TH_TI_ProviderReferenceID'. The duplicate key value is",
			},
		});

		public void TestImport_Insert_SamePk_HasRateWithSameProviderReferenceId_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "INSERT: should not insert the new one because of a constraint violation.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: Error from Data layer: TableName=RateEntry",
				"InnerException Message = Cannot insert duplicate key row in object 'dbo.RateEntry' with unique index 'NR_UX__TI_TH_TI_ProviderReferenceID'. The duplicate key value is",
			},
		});

		public void TestImport_Insert_SamePk_HasRateWithSameProviderReferenceId_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "INSERT: should expire old rate but should not insert the new one because of a constraint violation.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,31-Jan-16,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Existing RateEntry was expired by incoming RateEntry",
				"RateEndDate changed from value Empty date to value 31/01/2016 12:00:00 AM",
				"Test error: Error from Data layer: TableName=RateEntry",
				"InnerException Message = Cannot insert duplicate key row in object 'dbo.RateEntry' with unique index 'NR_UX__TI_TH_TI_ProviderReferenceID'. The duplicate key value is",
			},
		});

		public void TestImport_Insert_SamePk_HasRateWithSameProviderReferenceId_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "INSERT",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "INSERT: when there is a fully overlapped rate, should fail with error raised.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: Incoming RateEntry date range conflicts with existing RateEntry.",
				"Existing RateEntry date range: 01-Jan-16 00:00:00 - Empty date.",
				"Incoming RateEntry date range 01-Jan-16 00:00:00 - Empty date",
			}
		});

		#endregion

		#region UPDATE action

		public void TestImport_Update_NoPkInXml_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should not update the rate because it cannot be found with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: There is no RateEntry with the following values:",
				"Error occurred trying to import file. Please fix the error and try importing the file again.",
			},
		});

		public void TestImport_Update_NoPkInXml_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should not update the rate because it cannot be found with different date ranges.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: There is no RateEntry with the following values:",
				"Error occurred trying to import file. Please fix the error and try importing the file again.",
			},
		});

		public void TestImport_Update_NoPkInXml_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should not update the rate because it cannot be found with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: There is no RateEntry with the following values:",
				"Error occurred trying to import file. Please fix the error and try importing the file again.",
			},
		});

		public void TestImport_Update_NoPkInXml_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should find the rate but should not update it because of a date range error.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"RateEntry - 0 inserts, 0 updates, 0 deletes",
			},
		});

		public void TestImport_Update_NoPkInXml_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should not update the rate because it cannot be found with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: There is no RateEntry with the following values:",
			},
		});

		public void TestImport_Update_NoPkInXml_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should not update the rate because it cannot be found with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: There is no RateEntry with the following values:",
			},
		});

		public void TestImport_Update_NoPkInXml_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should not update the rate because it cannot be found with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: There is no RateEntry with the following values:",
			},
		});

		public void TestImport_Update_NoPkInXml_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should update ProviderReferenceIDs to blank.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Update_NoPkInXml_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should not update the rate because it cannot be found with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: There is no RateEntry with the following values:",
			},
		});

		public void TestImport_Update_NoPkInXml_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should not update the rate because it cannot be found with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: There is no RateEntry with the following values:",
			},
		});

		public void TestImport_Update_NoPkInXml_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should not update the rate because it cannot be found with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: There is no RateEntry with the following values:",
			},
		});

		public void TestImport_Update_NoPkInXml_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should update the rate with new ProviderReferenceIDs.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			}
		});

		public void TestImport_Update_NoPkInXml_HasRateWithDifferentProviderReferenceId_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "UPDATE: should not update the rate because it cannot be found with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: There is no RateEntry with the following values:",
			},
		});

		public void TestImport_Update_NoPkInXml_HasRateWithDifferentProviderReferenceId_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "UPDATE: should not update the rate because it cannot be found with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: There is no RateEntry with the following values:",
			},
		});

		public void TestImport_Update_NoPkInXml_HasRateWithDifferentProviderReferenceId_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "UPDATE: should not update the rate because it cannot be found with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: There is no RateEntry with the following values:",
			},
		});

		public void TestImport_Update_NoPkInXml_HasRateWithDifferentProviderReferenceId_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "UPDATE: should update the rate with new ProviderReferenceIDs.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Update_NoPkInXml_HasRateWithSameProviderReferenceId_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "UPDATE: should update the rate because it can be found with ProviderReferenceIDs",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Update_NoPkInXml_HasRateWithSameProviderReferenceId_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "UPDATE: should update the rate because it can be found with ProviderReferenceIDs",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Update_NoPkInXml_HasRateWithSameProviderReferenceId_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "UPDATE: should update the rate (to have new start date) because it can be found with ProviderReferenceIDs",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Update_NoPkInXml_HasRateWithSameProviderReferenceId_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "UPDATE: should find the rate with the ProviderReferenceIDs but there is no update because there is no change to it.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"RateEntry - 0 inserts, 0 updates, 0 deletes",
			}
		});

		///

		public void TestImport_Update_DifferentPk_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should not update the rate because it cannot be found with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: There is no RateEntry with the following values:",
			},
		});

		public void TestImport_Update_DifferentPk_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should not update the rate because it cannot be found with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: There is no RateEntry with the following values:",
			},
		});

		public void TestImport_Update_DifferentPk_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should not update the rate because it cannot be found with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: There is no RateEntry with the following values:",
			},
		});

		public void TestImport_Update_DifferentPk_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should find the rate with the ProviderReferenceIDs but there is no update because there is no change to it.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"RateEntry - 0 inserts, 0 updates, 0 deletes",
			}
		});

		public void TestImport_Update_DifferentPk_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should not update the rate because it cannot be found with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: There is no RateEntry with the following values:"
			},
		});

		public void TestImport_Update_DifferentPk_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should not update the rate because it cannot be found with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: There is no RateEntry with the following values:"
			},
		});

		public void TestImport_Update_DifferentPk_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should not update the rate because it cannot be found with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: There is no RateEntry with the following values:"
			},
		});

		public void TestImport_Update_DifferentPk_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should update the rate with new ProviderReferenceIDs.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Update_DifferentPk_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should not update the rate because it cannot be found with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: There is no RateEntry with the following values:"
			},
		});

		public void TestImport_Update_DifferentPk_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should not update the rate because it cannot be found with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: There is no RateEntry with the following values:"
			},
		});

		public void TestImport_Update_DifferentPk_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should not update the rate because it cannot be found with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: There is no RateEntry with the following values:"
			},
		});

		public void TestImport_Update_DifferentPk_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should update the rate with new ProviderReferenceIDs.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Update_DifferentPk_HasRateWithDifferentProviderReferenceId_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "UPDATE: should not update the rate because it cannot be found with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: There is no RateEntry with the following values:"
			},
		});

		public void TestImport_Update_DifferentPk_HasRateWithDifferentProviderReferenceId_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "UPDATE: should not update the rate because it cannot be found with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: There is no RateEntry with the following values:"
			},
		});

		public void TestImport_Update_DifferentPk_HasRateWithDifferentProviderReferenceId_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "UPDATE: should not update the rate because it cannot be found with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: There is no RateEntry with the following values:"
			},
		});

		public void TestImport_Update_DifferentPk_HasRateWithDifferentProviderReferenceId_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "UPDATE: should update the rate with new ProviderReferenceIDs.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Update_DifferentPk_HasRateWithSameProviderReferenceId_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "UPDATE: should update the rate because it can be found with ProviderReferenceIDs",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Update_DifferentPk_HasRateWithSameProviderReferenceId_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "UPDATE: update the rate because it can be found with ProviderReferenceIDs",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Update_DifferentPk_HasRateWithSameProviderReferenceId_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "UPDATE: should update the rate (to have new start date) because it can be found with ProviderReferenceIDs",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Update_DifferentPk_HasRateWithSameProviderReferenceId_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "UPDATE: should find the rate with the ProviderReferenceIDs but there is no update because there is no change to it.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"RateEntry - 0 inserts, 0 updates, 0 deletes",
			}
		});

		///

		public void TestImport_Update_SamePk_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should update the rate with blank ProviderReferenceIDs because it can be found with the PKs",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Update_SamePk_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should update the rate with blank ProviderReferenceIDs because it can be found with the PKs",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Update_SamePk_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should update the rate with blank ProviderReferenceIDs because it can be found with the PKs",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Update_SamePk_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should find with the PKs but there is no update because there is no change to it.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"RateEntry - 0 inserts, 0 updates, 0 deletes",
			}
		});

		public void TestImport_Update_SamePk_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should update the rate with new ProviderReferenceIDs because it can be found with the PKs.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Update_SamePk_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: update the rate with new date range because it can be found with the PKs.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Update_SamePk_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: update the rate with new date range because it can be found with the PKs.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Update_SamePk_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should update the rate with new ProviderReferenceIDs because it can be found with the PKs.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Update_SamePk_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should not update the rate because it cannot be found with ProviderReferenceID from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: There is no RateEntry with the following values:",
			},
		});

		public void TestImport_Update_SamePk_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should not update the rate because it cannot be found with ProviderReferenceID from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: There is no RateEntry with the following values:",
			},
		});

		public void TestImport_Update_SamePk_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should not update the rate because it cannot be found with ProviderReferenceID from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: There is no RateEntry with the following values:",
			},
		});

		public void TestImport_Update_SamePk_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "UPDATE: should update the rate with new ProviderReferenceIDs because it can be found with the differentiators.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Update_SamePk_HasRateWithDifferentProviderReferenceId_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "UPDATE: should not update the rate because it cannot be found with ProviderReferenceID from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: There is no RateEntry with the following values:",
			},
		});

		public void TestImport_Update_SamePk_HasRateWithDifferentProviderReferenceId_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "UPDATE: should not update the rate because it cannot be found with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: There is no RateEntry with the following values:",
			},
		});

		public void TestImport_Update_SamePk_HasRateWithDifferentProviderReferenceId_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "UPDATE: should not update the rate because it cannot be found with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Test error: There is no RateEntry with the following values:",
			},
		});

		public void TestImport_Update_SamePk_HasRateWithDifferentProviderReferenceId_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "UPDATE: should update the rate with new ProviderReferenceIDs because it can be found with the differentiators.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Update_SamePk_HasRateWithSameProviderReferenceId_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "UPDATE: should update the rate because it can be found with ProviderReferenceIDs",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Update_SamePk_HasRateWithSameProviderReferenceId_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "UPDATE: should update the rate because it can be found with ProviderReferenceIDs",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Update_SamePk_HasRateWithSameProviderReferenceId_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "UPDATE: should update the rate because it can be found with ProviderReferenceIDs",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Update_SamePk_HasRateWithSameProviderReferenceId_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "UPDATE",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "UPDATE: should find the rate with the ProviderReferenceIDs but there is no update because there is no change to it.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"RateEntry - 0 inserts, 0 updates, 0 deletes",
			}
		});

		#endregion

		#region MERGE action

		public void TestImport_Merge_NoPkInXml_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should insert new rate because cannot find the rate with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,|FRT,,DEF|0,10"
			},
		});

		public void TestImport_Merge_NoPkInXml_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should insert new rate because cannot find the rate with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Merge_NoPkInXml_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should expire the rate and insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,31-Jan-16,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Existing RateEntry was expired by incoming RateEntry",
				"RateEndDate changed from value Empty date to value 31/01/2016 12:00:00 AM",
			},
		});

		public void TestImport_Merge_NoPkInXml_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should find the rate but should not update it because there is no change.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"RateEntry - 0 inserts, 0 updates, 0 deletes",
			},
		});

		public void TestImport_Merge_NoPkInXml_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should insert new rate because cannot find the rate with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Merge_NoPkInXml_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should insert new rate because cannot find the rate with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Merge_NoPkInXml_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should expire the rate and insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,31-Jan-16,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Existing RateEntry was expired by incoming RateEntry",
				"RateEndDate changed from value Empty date to value 31/01/2016 12:00:00 AM",
			},
		});

		public void TestImport_Merge_NoPkInXml_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should update ProviderReferenceIDs to blank.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Merge_NoPkInXml_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should insert new rate because cannot find the rate with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Merge_NoPkInXml_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should insert new rate because cannot find the rate with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10"
			},
		});

		public void TestImport_Merge_NoPkInXml_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should expire the rate and insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,31-Jan-16,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Existing RateEntry was expired by incoming RateEntry",
				"RateEndDate changed from value Empty date to value 31/01/2016 12:00:00 AM",
			},
		});

		public void TestImport_Merge_NoPkInXml_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should update the rate with new ProviderReferenceIDs.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			}
		});

		public void TestImport_Merge_NoPkInXml_HasRateWithDifferentProviderReferenceId_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "MERGE: should insert new rate because cannot find the rate with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Merge_NoPkInXml_HasRateWithDifferentProviderReferenceId_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "MERGE: should insert new rate because cannot find the rate with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Merge_NoPkInXml_HasRateWithDifferentProviderReferenceId_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "MERGE: should expire the rate and insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,31-Jan-16,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Existing RateEntry was expired by incoming RateEntry",
				"RateEndDate changed from value Empty date to value 31/01/2016 12:00:00 AM",
			},
		});

		public void TestImport_Merge_NoPkInXml_HasRateWithDifferentProviderReferenceId_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "MERGE: should update the rate with new ProviderReferenceIDs.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Merge_NoPkInXml_HasRateWithSameProviderReferenceId_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "MERGE: should update the rate because it can be found with ProviderReferenceIDs",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Merge_NoPkInXml_HasRateWithSameProviderReferenceId_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "MERGE: should update the rate because it can be found with ProviderReferenceIDs",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Merge_NoPkInXml_HasRateWithSameProviderReferenceId_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "MERGE: should update the rate (to have new start date) because it can be found with ProviderReferenceIDs",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Merge_NoPkInXml_HasRateWithSameProviderReferenceId_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.NoPkInXml,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "MERGE: should find the rate with the ProviderReferenceIDs but there is no update because there is no change to it.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"RateEntry - 0 inserts, 0 updates, 0 deletes",
			}
		});

		///

		public void TestImport_Merge_DifferentPk_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should insert new rate because cannot find the rate with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Merge_DifferentPk_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should insert new rate because cannot find the rate with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Merge_DifferentPk_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should expire the rate and insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,31-Jan-16,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Existing RateEntry was expired by incoming RateEntry",
				"RateEndDate changed from value Empty date to value 31/01/2016 12:00:00 AM",
			},
		});

		public void TestImport_Merge_DifferentPk_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should find the rate by matching differentiators but there is no update because there is no change to it.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"RateEntry - 0 inserts, 0 updates, 0 deletes",
			}
		});

		public void TestImport_Merge_DifferentPk_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should insert new rate because cannot find the rate with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Merge_DifferentPk_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Merge_DifferentPk_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should not update the rate because it cannot be found with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Jan-16,31-Jan-16,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Existing RateEntry was expired by incoming RateEntry",
				"RateEndDate changed from value Empty date to value 31/01/2016 12:00:00 AM",
			},
		});

		public void TestImport_Merge_DifferentPk_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should update the rate with new ProviderReferenceIDs.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Merge_DifferentPk_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should insert new rate because cannot find the rate with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Merge_DifferentPk_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should insert new rate because cannot find the rate with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Merge_DifferentPk_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should expire the rate and insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,31-Jan-16,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Existing RateEntry was expired by incoming RateEntry",
				"RateEndDate changed from value Empty date to value 31/01/2016 12:00:00 AM",
			},
		});

		public void TestImport_Merge_DifferentPk_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should update the rate with new ProviderReferenceIDs.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Merge_DifferentPk_HasRateWithDifferentProviderReferenceId_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "MERGE: should insert new rate because cannot find the rate with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Merge_DifferentPk_HasRateWithDifferentProviderReferenceId_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "MERGE: should insert new rate because cannot find the rate with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Merge_DifferentPk_HasRateWithDifferentProviderReferenceId_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "MERGE: should expire the rate and insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,31-Jan-16,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Existing RateEntry was expired by incoming RateEntry",
				"RateEndDate changed from value Empty date to value 31/01/2016 12:00:00 AM",
			},
		});

		public void TestImport_Merge_DifferentPk_HasRateWithDifferentProviderReferenceId_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "MERGE: should update the rate with new ProviderReferenceIDs.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Merge_DifferentPk_HasRateWithSameProviderReferenceId_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "MERGE: should update the rate because it can be found with ProviderReferenceIDs",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Merge_DifferentPk_HasRateWithSameProviderReferenceId_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "MERGE: update the rate because it can be found with ProviderReferenceIDs",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Merge_DifferentPk_HasRateWithSameProviderReferenceId_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "MERGE: should update the rate (to have new start date) because it can be found with ProviderReferenceIDs",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Merge_DifferentPk_HasRateWithSameProviderReferenceId_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasDifferentPkThanXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "MERGE: should find the rate with the ProviderReferenceIDs but there is no update because there is no change to it.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"RateEntry - 0 inserts, 0 updates, 0 deletes",
			}
		});

		///

		public void TestImport_Merge_SamePk_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should update the rate with blank ProviderReferenceIDs because it can be found with the PKs",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Merge_SamePk_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should update the rate with blank ProviderReferenceIDs because it can be found with the PKs",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Merge_SamePk_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should update the rate with blank ProviderReferenceIDs because it can be found with the PKs",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Merge_SamePk_NoProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should find with the PKs but there is no update because there is no change to it.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"RateEntry - 0 inserts, 0 updates, 0 deletes",
			}
		});

		public void TestImport_Merge_SamePk_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should update the rate with new ProviderReferenceIDs because it can be found with the PKs.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Merge_SamePk_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "MERGE: update the rate with new date range because it can be found with the PKs.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Merge_SamePk_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "MERGE: update the rate with new date range because it can be found with the PKs.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Merge_SamePk_NoProviderReferenceIdInXml_HasProviderReferenceIdInRate_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should update the rate with new ProviderReferenceIDs because it can be found with the PKs.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
			},
		});

		public void TestImport_Merge_SamePk_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should insert new rate because cannot find the rate with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Merge_SamePk_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should insert new rate because cannot find the rate with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Merge_SamePk_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should expire the rate and insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,31-Jan-16,111,AUSYD,|FRT,,DEF|0,10",
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Existing RateEntry was expired by incoming RateEntry",
				"RateEndDate changed from value Empty date to value 31/01/2016 12:00:00 AM",
			},
		});

		public void TestImport_Merge_SamePk_HasProviderReferenceIdInXml_NoProviderReferenceIdInRate_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate,
			AssertionMessage = "MERGE: should update the rate with new ProviderReferenceIDs because it can be found with the PKs.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Merge_SamePk_HasRateWithDifferentProviderReferenceId_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "MERGE: should insert new rate because cannot find the rate with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Merge_SamePk_HasRateWithDifferentProviderReferenceId_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "MERGE: should insert new rate because cannot find the rate with data from XML.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Merge_SamePk_HasRateWithDifferentProviderReferenceId_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "MERGE: should expire the rate and insert the new one.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,31-Jan-16,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"Existing RateEntry was expired by incoming RateEntry",
				"RateEndDate changed from value Empty date to value 31/01/2016 12:00:00 AM",
			},
		});

		public void TestImport_Merge_SamePk_HasRateWithDifferentProviderReferenceId_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId,
			AssertionMessage = "MERGE: should update the rate with new ProviderReferenceIDs because it can be found with the differentiators.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,NEW_ENTRY_ID|FRT,NEW_LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Merge_SamePk_HasRateWithSameProviderReferenceId_NoMatchingRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.NoMatching,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "MERGE: should update the rate because it can be found with ProviderReferenceIDs",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,SGSIN,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Merge_SamePk_HasRateWithSameProviderReferenceId_MatchingNoOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.NoOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "MERGE: should update the rate because it can be found with ProviderReferenceIDs",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-15,01-Feb-15,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Merge_SamePk_HasRateWithSameProviderReferenceId_MatchingPartiallyOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.PartiallyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "MERGE: should update the rate because it can be found with ProviderReferenceIDs",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Feb-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
		});

		public void TestImport_Merge_SamePk_HasRateWithSameProviderReferenceId_MatchingFullyOverlappedRate() => TestImport(new TestCase
		{
			Action = "MERGE",
			DbRateSetup = DbRateSetupFlags.FullyOverlappedDate,
			PkSetup = PkSetupFlags.RateHasSamePkAsTheXmlOne,
			ProviderReferenceSetup = ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId,
			AssertionMessage = "MERGE: should find the rate with the ProviderReferenceIDs but there is no update because there is no change to it.",
			ExpectedRates = new[]
			{
				"EDI,QUETUB|01-Jan-16,,111,AUSYD,ENTRY_ID|FRT,LINE_ID,DEF|0,10",
			},
			ExpectedLogStrings = new[]
			{
				"RateEntry - 0 inserts, 0 updates, 0 deletes",
			}
		});

		#endregion

		void TestImport(TestCase testcase)
		{
			// Test setup: prepare an existing rate in DB.
			var testData = RateTestData.NewWithValidTestData();
			if (testcase.ProviderReferenceSetup is
				ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate or
				ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate)
			{
				testData.Replace("{RATEENTRY_PROVIDER_REFERENCE_ID}", string.Empty);
				testData.Replace("{RATELINE_PROVIDER_REFERENCE_ID}", string.Empty);
			}

			var (template, xml) = testData.GenerateXmlFromTemplateFile(ProviderRefIdTestFile);
			Import(xml);

			testData.Replace("{RATEENTRY_ACTION}", testcase.Action);
			testData.Replace("{RATELINE_ACTION}", testcase.Action);

			switch (testcase.DbRateSetup)
			{
				case DbRateSetupFlags.NoMatching:
					// Origin is a differentiator so changing it makes the rates not match.
					testData.Replace("{ORIGIN}", "SGSIN");
					break;

				case DbRateSetupFlags.FullyOverlappedDate:
					// The new date range is the same with the existing rate in the DB.
					break;

				case DbRateSetupFlags.PartiallyOverlappedDate:
					// The new date range is partially overlapped with the existing rate in the DB:
					// | 01-01-2016 |-------------------->
					// --------------- | 01-02-2016 |---->
					testData.Replace("{RATE_START_DATE}", "2016-02-01T00:00:00");
					break;

				case DbRateSetupFlags.NoOverlappedDate:
					// The new date range is not overlapped with the existing rate in the DB.
					// ----------------------------------| 01-01-2016 |----->
					// | 01-01-2015 |--| 01-02-2015 |
					testData.Replace("{RATE_START_DATE}", "2015-01-01T00:00:00");
					testData.Replace("{RATE_END_DATE}", "2015-02-01T00:00:00");
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}

			switch (testcase.PkSetup)
			{
				case PkSetupFlags.NoPkInXml:
					testData.Replace("{RATINGHEADER_PK}", string.Empty);
					testData.Replace("{RATEENTRY_PK}", string.Empty);
					testData.Replace("{RATELINE_PK}", string.Empty);
					break;

				case PkSetupFlags.RateHasDifferentPkThanXmlOne:
					testData.Replace("{RATEENTRY_PK}", ToPkTag("e8459fb5-4173-4bbd-bc86-92d38dfb202e"));
					testData.Replace("{RATELINE_PK}", ToPkTag("a3badb0a-c535-4d95-87b1-7078f0dd9ea4"));
					break;

				case PkSetupFlags.RateHasSamePkAsTheXmlOne:
					// Reapply the actual PKs from DB to the new ones in XML
					var ratingHeader = factory.Load<RatingHeader>(new ZQuery())[0];
					testData.Replace("{RATINGHEADER_PK}", ToPkTag(ratingHeader.PK));

					var rateEntry = ratingHeader.AllEntries.Single();
					testData.Replace("{RATEENTRY_PK}", ToPkTag(rateEntry.PK));
					var rateLine = rateEntry.RateLines.Single();
					testData.Replace("{RATELINE_PK}", ToPkTag(rateLine.PK));
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}

			switch (testcase.ProviderReferenceSetup)
			{
				case ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_NoProviderReferenceIdInRate:
				case ProviderReferenceSetupFlags.NoProviderReferenceIdInXml_HasProviderReferenceIdInRate:
					testData.Replace("{RATEENTRY_PROVIDER_REFERENCE_ID}", string.Empty);
					testData.Replace("{RATELINE_PROVIDER_REFERENCE_ID}", string.Empty);
					break;

				case ProviderReferenceSetupFlags.HasProviderReferenceIdInXml_NoProviderReferenceIdInRate:
				case ProviderReferenceSetupFlags.HasRateWithDifferentProviderReferenceId:
					testData.Replace("{RATEENTRY_PROVIDER_REFERENCE_ID}", "NEW_ENTRY_ID");
					testData.Replace("{RATELINE_PROVIDER_REFERENCE_ID}", "NEW_LINE_ID");
					break;

				case ProviderReferenceSetupFlags.HasRateWithSameProviderReferenceId:
					// Keep the IDs the same as the existing rate in the DB.
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}

			xml = testData.GenerateXmlFromTemplate(template);
			Import(xml);

			var actualLogMessages = GetLogsMessage();
			var assertionMessage = testcase.AssertionMessage + "\n" + actualLogMessages;

			var allRatingHeaders = factory.CreateNewFactory().Load<RatingHeader>(new ZQuery());
			AssertContainsExactElementsInAnyOrder(assertionMessage, testcase.ExpectedRates, allRatingHeaders.SelectMany(ToStrings));

			CombineAssertions("Log should contain the expected strings", () =>
			{
				testcase.ExpectedLogStrings?.ForEach(expectedLogString =>
				{
					AssertContains(expectedLogString, actualLogMessages);
				});

				if (testcase.ExpectNotHavingError)
				{
					AssertNotContains("Error", actualLogMessages, ignoreCase: true);
				}
			});
		}

		void Import(string xml)
		{
			using MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(xml));
			manager.Import(stream);
		}

		const string ProviderRefIdTestFile = "Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.ClientRate_ProviderRefIdWithActions.xml";

		static string ToPkTag(string pk) => $"<PK>{pk}</PK>";
		static string ToPkTag(ZGuid pk) => ToPkTag(pk.ToString());

		/// <summary>
		/// Each string represents a RateLineItem and its parent RateLine, RateEntry, and RatingHeader.
		/// </summary>
		static IEnumerable<string> ToStrings(RatingHeader ratingHeader)
		{
			var rateEntries = ratingHeader.AllEntriesCollection.OfType<RateEntry>().ToArray();
			foreach (var rateEntry in rateEntries)
			{
				var rateLines = rateEntry.RateLines.OfType<RateLine>().ToArray();
				if (rateLines.Length == 0)
				{
					yield return ToString(ratingHeader, rateEntry);
					continue;
				}

				foreach (var rateLine in rateLines)
				{
					var rateLineItems = rateLine.RateLineItems.OfType<RateLineItem>().ToArray();
					if (rateLineItems.Length == 0)
					{
						yield return ToString(ratingHeader, rateEntry, rateLine);
						continue;
					}

					foreach (var rateLineItem in rateLineItems)
					{
						yield return ToString(ratingHeader, rateEntry, rateLine, rateLineItem);
					}
				}
			}
		}

		static string ToString(RatingHeader ratingHeader, RateEntry rateEntry, RateLine rateLine = null, RateLineItem rateLineItem = null)
		{
			var result = new StringBuilder();

			result
				.Append(ratingHeader.Company.GC_Code).Append(",").Append(ratingHeader.Header.OH_Code).Append("|")
				.Append(rateEntry.TI_RateStartDate).Append(",").Append(rateEntry.TI_RateEndDate).Append(",")
				.Append(rateEntry.TI_ContractNumber).Append(",").Append(rateEntry.TI_OriginLRC).Append(",").Append(rateEntry.TI_ProviderReferenceID).Append("|");

			if (rateLine == null)
			{
				result.Append("No RateLine|");
			}
			else
			{
				result.Append(rateLine.ChargeCode.AC_Code).Append(",").Append(rateLine.TL_ProviderReferenceID).Append(",").Append(rateLine.TL_Rounding).Append("|");
			}

			if (rateLineItem == null)
			{
				result.Append("No RateLineItem");
			}
			else
			{
				result.Append(rateLineItem.TM_BreakMinimum.ToString(0)).Append(",").Append(rateLineItem.TM_Value.ToString(0));
			}

			return result.ToString();
		}

		string GetLogsMessage()
		{
			return string.Join(System.Environment.NewLine, dummyLogger.Buffer.Logs().Select(log => log.Message));
		}

		void ErrorOccur(XElement source, Exception ex)
		{
			dummyLogger.Error("Test error: " + ex.Message);
		}

		protected override void SetUp()
		{
			base.SetUp();

			sessionServices = new AncillaryImportServices();
			dummyLogger = sessionServices.Logger as MemoryLogger;
			manager = new ImportHandler(sessionServices)
			{
				ErrorOccur = ErrorOccur
			};
			factory = new BusinessObjectFactory();

			demoCompanyPK = factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM")).PK.ToGuid();
			var query = new ZQuery(GlbBranchSchema.GB_GC, demoCompanyPK);
			query.AddToFilter(GlbBranchSchema.GB_Code, "DEM");
		}

		AncillaryImportServices sessionServices;
		MemoryLogger dummyLogger;
		ImportHandler manager;
		BusinessObjectFactory factory;
		Guid demoCompanyPK;
	}
}
