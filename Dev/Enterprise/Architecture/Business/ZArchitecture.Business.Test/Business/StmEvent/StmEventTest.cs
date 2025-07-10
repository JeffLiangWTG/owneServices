using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(StmEvent))]
	sealed class StmEventTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPropagation_ReadOnly()
		{
			var stmEvent = Factory.New<StmEvent>();
			stmEvent.SE_Code = Events.AddedARecordToTheSystemCode;
			AssertEquals(true, stmEvent.SE_PropagateToParentInfo.ReadOnly);
			AssertEquals(true, stmEvent.SE_PropagateKeyedByEventAndReferenceInfo.ReadOnly);
			stmEvent.SE_Code = Events.CustomisableEvent00Code;
			AssertEquals(false, stmEvent.SE_PropagateToParentInfo.ReadOnly);
			AssertEquals(false, stmEvent.SE_PropagateKeyedByEventAndReferenceInfo.ReadOnly);
			stmEvent.SE_Code = Events.OceanCarrierBookingByTEUCode;
			AssertEquals(true, stmEvent.SE_PropagateToParentInfo.ReadOnly);
			AssertEquals(true, stmEvent.SE_PropagateKeyedByEventAndReferenceInfo.ReadOnly);
			stmEvent.SE_Code = Events.BillStatusUpdatedCode;
			AssertEquals(true, stmEvent.SE_PropagateToParentInfo.ReadOnly);
			AssertEquals(true, stmEvent.SE_PropagateKeyedByEventAndReferenceInfo.ReadOnly);
		}

		public void TestOverridenReferenceFormat_ReadOnly()
		{
			var stmEvent = Factory.New<StmEvent>();
			stmEvent.SE_Code = Events.BillStatusUpdatedCode;
			AssertEquals(true, stmEvent.SE_IsRefernceFormatOverriddenInfo.ReadOnly);
			stmEvent.SE_Code = Events.StatusUpdatedCode;
			AssertEquals(false, stmEvent.SE_IsRefernceFormatOverriddenInfo.ReadOnly);
		}

		public void TestPropagation_NeverPropagateAuditEvents()
		{
			var stmEvent = Factory.New<StmEvent>();
			stmEvent.SE_PropagateToParent = true;
			foreach (var code in Events.ChangeLogCodes)
			{
				stmEvent.SE_Code = code;
				AssertEquals(false, stmEvent.SE_PropagateToParent);
				AssertEquals(false, stmEvent.SE_PropagateKeyedByEventAndReference);
			}
		}

		public void TestDescription()
		{
			var stmEvent = Factory.New<StmEvent>();
			stmEvent.SE_Code = Events.BookedCode;
			AssertEquals(Events.Booked.Description, stmEvent.SE_Desc);
			using (var mockRes = Res.UseMockData())
			{
				string resKey = ((ResourceString)Events.Booked.MultilingualDescription).ResourceKey;
				mockRes.Put(resKey, new ResourceStringData(resKey, "Dekoob"));

				AssertEquals("Dekoob", stmEvent.SE_Desc);
			}
		}

		public void TestEventCastOperator()
		{
			var stmEvent = Factory.New<StmEvent>();
			stmEvent.SE_Code = "ZZZ";
			stmEvent.SE_Desc = "A small description noone will read";

			var evt = (Event)stmEvent;
			AssertEquals(stmEvent.SE_Code, evt.Code);
			AssertEquals(stmEvent.SE_DescMultilingual, evt.MultilingualDescription);
			AssertEquals(stmEvent.PK, evt.PK);
		}

		public void TestSE_IsActive_IsReadOnly()
		{
			var stmEvent = Factory.New<StmEvent>();
			Assert(stmEvent.SE_IsActiveInfo.ReadOnly);
		}

		public void TestSE_IsCustomizable_IsReadOnly()
		{
			var stmEvent = Factory.New<StmEvent>();
			Assert(stmEvent.SE_IsCustomizableInfo.ReadOnly);
		}

		public void TestInactiveEventsDescriptionShouldContainReplacementEventCodes()
		{
			var inactiveEventsWithoutReplacement = new[]
				{
					Events.RelatedNotesNotReadCode
				};

			var replacedBySingleEventRegex = new Regex("(Replaced by ([A-Z0-9]{3}) Event( with ([A-Z0-9]{3}) parameter)?)$");
			var replacedByDoubleEventsRegex = new Regex("(Replaced by ([A-Z0-9]{3}) and ([A-Z0-9]{3}) Events)$");

			var inactiveEventsWithIncorrectDescriptions = new List<StmEvent>();

			var allInactiveEvents = Factory.Load<StmEvent>(new ZQuery(StmEventSchema.SE_IsActive, false));
			foreach (var inactiveEvent in allInactiveEvents)
			{
				if (!inactiveEventsWithoutReplacement.Contains(inactiveEvent.SE_Code.ToString())
					&& !replacedBySingleEventRegex.IsMatch(inactiveEvent.SE_Desc)
					&& !replacedByDoubleEventsRegex.IsMatch(inactiveEvent.SE_Desc))
				{
					inactiveEventsWithIncorrectDescriptions.Add(inactiveEvent);
				}
			}

			if (inactiveEventsWithIncorrectDescriptions.Any())
			{
				var builder = new ZStringBuilder();
				builder.Append("All inactive events descriptions should end with 'Replaced by' statements, indicating replacement event codes");
				builder.Append("- when replaced with a single event: Replaced by XXX Event or Replaced by XXX Event with YYY parameter");
				builder.Append("- when replaced with a double events: Replaced by XXX and YYY Events");
				builder.Append("Where XXX, YYY are event codes of the replacement events");
				builder.Append("");
				builder.Append("But this event(s) have incorrect descriptions:");

				foreach (var inactiveEventWithIncorrectDescription in inactiveEventsWithIncorrectDescriptions)
				{
					builder.Append(string.Format("{0} - {1}", inactiveEventWithIncorrectDescription.SE_Code, inactiveEventWithIncorrectDescription.SE_Desc));
				}

				Fail(builder.ToStringWithNewLineBetweenAppends());
			}

			Assert(true);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}
	}
}
