using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class BulkDeliveryMethodListTest : TestCaseWithFactory
	{
		public void TestAllowDeliverDocumentsInOneEmail()
		{
			CombineAssertions(delegate
			{
				AssertEquals("Auto", false, List.AllowDeliverDocumentsInOneEmail(AutoBulkDeliveryMethod.CodeText));
				AssertEquals("Only Printer", false, List.AllowDeliverDocumentsInOneEmail(OnlyPrinterBulkDeliveryMethod.CodeText));
				AssertEquals("Force Printer", false, List.AllowDeliverDocumentsInOneEmail(ForcePrinterBulkDeliveryMethod.CodeText));
				AssertEquals("Only Electronic", true, List.AllowDeliverDocumentsInOneEmail(OnlyElectronicBulkDeliveryMethod.CodeText));
				AssertEquals("Invalid", false, List.AllowDeliverDocumentsInOneEmail("XXX"));
			});
		}

		public void TestCodeNotDuplicated()
		{
			Dictionary<string, int> counts = new Dictionary<string, int>();
			foreach (CodeDescriptionPair pair in List)
			{
				int count;
				if (counts.TryGetValue(pair.Code, out count))
				{
					counts[pair.Code] = count + 1;
				}
				else
				{
					counts.Add(pair.Code, 1);
				}
			}

			StringBuilder builder = new StringBuilder();
			foreach (KeyValuePair<string, int> pair in counts)
			{
				if (pair.Value > 1)
				{
					builder.AppendLine(pair.Key);
				}
			}

			AssertEquals("These codes appear more than once", "", builder.ToString());
		}

		public void TestUsePrinter()
		{
			CombineAssertions(delegate
			{
				AssertEquals("Only Printer", true, List.UsesPrinter(OnlyPrinterBulkDeliveryMethod.CodeText));
				AssertEquals("Only Electronic", false, List.UsesPrinter(OnlyElectronicBulkDeliveryMethod.CodeText));
				AssertEquals("Invalid", true, List.UsesPrinter("XXX"));
			});
		}

		public void TestAllowCoverNote()
		{
			CombineAssertions(delegate
			{
				AssertEquals("Only Printer", false, List.AllowCoverNote(OnlyPrinterBulkDeliveryMethod.CodeText));
				AssertEquals("Only Electronic", true, List.AllowCoverNote(OnlyElectronicBulkDeliveryMethod.CodeText));
				AssertEquals("Invalid", false, List.AllowCoverNote("XXX"));
			});
		}

		public void TestGetByValidCode()
		{
			AssertEquals(typeof(OnlyElectronicBulkDeliveryMethod), List[OnlyElectronicBulkDeliveryMethod.CodeText].GetType());
		}

		[ExpectException(typeof(ArgumentOutOfRangeException))]
		public void TestGetByInvalidCode()
		{
			object blah = List["XXX"];
		}

		#region Implementation
		BulkDeliveryMethodList List
		{
			get
			{
				return list ?? (list = new BulkDeliveryMethodList());
			}
		}

		BulkDeliveryMethodList list;
		#endregion
	}
}
