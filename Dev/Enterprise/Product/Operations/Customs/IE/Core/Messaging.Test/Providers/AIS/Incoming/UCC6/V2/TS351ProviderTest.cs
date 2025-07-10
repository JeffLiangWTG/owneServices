using System;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS351;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	[TestedType(typeof(TS351Provider))]
	sealed class TS351ProviderTest : TestCaseWithFactory
	{
		public void TestLocalReferenceNumber()
		{
			AssertEquals("LRN001", provider.LocalReferenceNumber);
		}

		public void TestMovementReferenceNumber()
		{
			AssertEquals("12MRN345ABCDE678R9", provider.MovementReferenceNumber);
		}

		public void TestSpecificCircumstanceIndicator()
		{
			AssertEquals("SCI", provider.SpecificCircumstanceIndicator);
		}

		public void TestDateAndTimesOfPresentationOfTheGoods()
		{
			var pogDates = provider.DateAndTimesOfPresentationOfTheGoods.ToArray();
			AssertEquals(2, pogDates.Length);
			CombineAssertions("Normal DateTime", () =>
			{
				AssertEquals("Date 1", new DateTime(2023, 08, 10, 14, 30, 45), pogDates[0]);
				AssertEquals("Date 2", new DateTime(2023, 08, 11, 14, 30, 45), pogDates[1]);
			});

			provider = new TS351Provider(new Ts351
			{
				Declaration = new DeclarationType351
				{
					PreviousDocument = new System.Collections.ObjectModel.Collection<PreviousdocumentType08>
					{
						new PreviousdocumentType08 { DateAndTimeOfPresentationOfTheGoods = DateTime.MinValue },
						new PreviousdocumentType08 { DateAndTimeOfPresentationOfTheGoods = DateTime.MinValue },
					}
				}
			});
			pogDates = provider.DateAndTimesOfPresentationOfTheGoods.ToArray();
			CombineAssertions("DateTime.MinValue", () =>
			{
				AssertEquals("Date 1", ZDateTime.Empty, pogDates[0]);
				AssertEquals("Date 2", ZDateTime.Empty, pogDates[1]);
			});
		}

		public void TestControlResultCode()
		{
			AssertEquals("CR", provider.ControlResultCode);
		}

		public void TestControlResultDate()
		{
			AssertEquals("Normal DateTime", new DateTime(2023, 08, 15, 14, 30, 45), provider.ControlResultDate);

			provider = new TS351Provider(new Ts351 { Declaration = new DeclarationType351 { ControlResult = new MControlResultType06 { Date = DateTime.MinValue } } });
			AssertEquals("DateTime.MinValue", ZDateTime.Empty, provider.ControlResultDate);
		}

		public void TestControlResultRemarks()
		{
			AssertEquals("Control Result Remarks", provider.ControlResultRemarks);
		}

		public void TestRemarks()
		{
			AssertEquals("Remarks001", provider.Remarks);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new TS351Provider(new Ts351
			{
				Declaration = new DeclarationType351
				{
					Lrn = "LRN001",
					Mrn = "12MRN345ABCDE678R9",
					SpecificCircumstanceIndicator = "SCI",
					PreviousDocument = new System.Collections.ObjectModel.Collection<PreviousdocumentType08>
					{
						new PreviousdocumentType08 { DateAndTimeOfPresentationOfTheGoods = new DateTime(2023, 08, 10, 14, 30, 45) },
						new PreviousdocumentType08 { DateAndTimeOfPresentationOfTheGoods = new DateTime(2023, 08, 11, 14, 30, 45) },
					},
					ControlResult = new MControlResultType06
					{
						Code = "CR",
						Date = new DateTime(2023, 08, 15, 14, 30, 45),
						Remarks = "Control Result Remarks",
					},
					Remarks = "Remarks001",
				},
			});
		}
		TS351Provider provider;
	}
}
