using System;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Freight.Agency.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(DetentionWrapper))]
	sealed class DetentionWrapperTest : GenericWrapperTest
	{
		public void TestOverdueDetention()
		{
			ZDateTime now = ZDateTime.Now;
			AgencyRegistry.Instance.DetentionAdviceWarningDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 7);

			DetentionWrapper wrapper = new DetentionWrapper(ZDateTime.Empty, now.AddDays(5), ZDateTime.Empty, ZInt.Zero, "AUSYD", Factory);
			AssertEquals("OverdueDays", 0, wrapper.OverdueDays);
			AssertEquals("wrapper.IsNearDue", true, wrapper.IsNearDue);
			AssertEquals("wrapper.IsNotDue", false, wrapper.IsNotDue);
			AssertEquals("wrapper.IsOverdue", false, wrapper.IsOverdue);

			wrapper = new DetentionWrapper(ZDateTime.Empty, now.AddDays(10), ZDateTime.Empty, ZInt.Zero, "AUSYD", Factory);
			AssertEquals("OverdueDays", 0, wrapper.OverdueDays);
			AssertEquals("wrapper.IsNearDue", false, wrapper.IsNearDue);
			AssertEquals("wrapper.IsNotDue", true, wrapper.IsNotDue);
			AssertEquals("wrapper.IsOverdue", false, wrapper.IsOverdue);

			wrapper = new DetentionWrapper(ZDateTime.Empty, now.AddDays(-5), ZDateTime.Empty, ZInt.Zero, "AUSYD", Factory);
			AssertEquals("OverdueDays", 5, wrapper.OverdueDays);
			AssertEquals("wrapper.IsNearDue", false, wrapper.IsNearDue);
			AssertEquals("wrapper.IsNotDue", false, wrapper.IsNotDue);
			AssertEquals("wrapper.IsOverdue", true, wrapper.IsOverdue);

			wrapper = new DetentionWrapper(ZDateTime.Empty, now.AddDays(15), now.AddDays(22), ZInt.Zero, "AUSYD", Factory);
			AssertEquals("OverdueDays", 7, wrapper.OverdueDays);
			AssertEquals("wrapper.IsNearDue", false, wrapper.IsNearDue);
			AssertEquals("wrapper.IsNotDue", false, wrapper.IsNotDue);
			AssertEquals("wrapper.IsOverdue", true, wrapper.IsOverdue);
		}

		public override void TestWrapperMappingsEmpty()
		{
			CombineAssertions(delegate
			{
				DetentionWrapper wrapper = new DetentionWrapper(ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, Factory);
				AssertEquals("Released", ZDateTime.Empty, wrapper.Released);
				AssertEquals("LastFreeDay", ZDateTime.Empty, wrapper.LastFreeDay);
				AssertEquals("Returned", ZDateTime.Empty, wrapper.Returned);
				AssertEquals("FreeDays", 0, wrapper.FreeDays);
				AssertEquals("DetentionDays", 0, wrapper.DetentionDays);
				AssertEquals("FormattedDetentionDays", "", wrapper.FormattedDetentionDays);
			});
		}

		[TestDate(2000, 1, 20)]
		public void TestWrapperMappingsFull()
		{
			CombineAssertions(delegate
			{
				DetentionWrapper wrapper = new DetentionWrapper(new ZDateTime(2000, 1, 1), new ZDateTime(2000, 1, 10), new ZDateTime(2000, 1, 15), Factory);
				AssertEquals("Released", new ZDateTime(2000, 1, 1), wrapper.Released);
				AssertEquals("LastFreeDay", new ZDateTime(2000, 1, 10), wrapper.LastFreeDay);
				AssertEquals("Returned", new ZDateTime(2000, 1, 15), wrapper.Returned);
				AssertEquals("FreeDays", 10, wrapper.FreeDays);
				AssertEquals("DetentionDays", 5, wrapper.DetentionDays);
				AssertEquals("FormattedDetentionDays", "5 days", wrapper.FormattedDetentionDays);
			});
		}

		[TestDate(2000, 1, 20)]
		public void TestWrapperMappingsEarly()
		{
			CombineAssertions(delegate
			{
				DetentionWrapper wrapper = new DetentionWrapper(new ZDateTime(2000, 1, 1), new ZDateTime(2000, 1, 10), new ZDateTime(2000, 1, 9), Factory);
				AssertEquals("Released", new ZDateTime(2000, 1, 1), wrapper.Released);
				AssertEquals("LastFreeDay", new ZDateTime(2000, 1, 10), wrapper.LastFreeDay);
				AssertEquals("Returned", new ZDateTime(2000, 1, 9), wrapper.Returned);
				AssertEquals("FreeDays", 10, wrapper.FreeDays);
				AssertEquals("DetentionDays", 0, wrapper.DetentionDays);
				AssertEquals("FormattedDetentionDays", "", wrapper.FormattedDetentionDays);
			});
		}

		[TestDate(2000, 1, 20)]
		public void TestWrapperMappingsNotReturned()
		{
			CombineAssertions(delegate
			{
				DetentionWrapper wrapper = new DetentionWrapper(new ZDateTime(2000, 1, 1), new ZDateTime(2000, 1, 10), ZDateTime.Empty, Factory);
				AssertEquals("Released", new ZDateTime(2000, 1, 1), wrapper.Released);
				AssertEquals("LastFreeDay", new ZDateTime(2000, 1, 10), wrapper.LastFreeDay);
				AssertEquals("Returned", ZDateTime.Empty, wrapper.Returned);
				AssertEquals("FreeDays", 10, wrapper.FreeDays);
				AssertEquals("DetentionDays", 0, wrapper.DetentionDays);
				AssertEquals("FormattedDetentionDays", "", wrapper.FormattedDetentionDays);
			});
		}

		[TestDate(2000, 1, 20)]
		public void TestWrapperMappingsDetentionDaysOverride()
		{
			CombineAssertions(delegate
			{
				DetentionWrapper wrapper = new DetentionWrapper(new ZDateTime(2000, 1, 1), new ZDateTime(2000, 1, 10), new ZDateTime(2000, 1, 15), 1, "AUSYD", Factory);
				AssertEquals("Released", new ZDateTime(2000, 1, 1), wrapper.Released);
				AssertEquals("LastFreeDay", new ZDateTime(2000, 1, 10), wrapper.LastFreeDay);
				AssertEquals("Returned", new ZDateTime(2000, 1, 15), wrapper.Returned);
				AssertEquals("FreeDays", 10, wrapper.FreeDays);
				AssertEquals("DetentionDays", 1, wrapper.DetentionDays);
				AssertEquals("FormattedDetentionDays", "1 day", wrapper.FormattedDetentionDays);
				AssertEquals("Port", "AUSYD", wrapper.Port.UNLOCO);
			});
		}

		#region Implementation

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Empty : 
Port : 
Registry : (No Default Field Value Available on Registry)
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new DetentionWrapper(ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Detention Information          (Default Field: FormattedDetentionDays)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Empty                                   Detention Information
Port                                    Location
DetentionDays                           Int
FormattedDetentionDays                  String
FreeDays                                Int
IsNearDue                               Bool
IsNotDue                                Bool
IsOverdue                               Bool
LastFreeDay                             DateTime
OverdueDays                             Int
Released                                DateTime
Returned                                DateTime
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new DetentionWrapper(new ZDateTime(2000, 1, 1), new ZDateTime(2000, 1, 11), new ZDateTime(2000, 1, 16), Factory);
		}

		#endregion
	}
}
