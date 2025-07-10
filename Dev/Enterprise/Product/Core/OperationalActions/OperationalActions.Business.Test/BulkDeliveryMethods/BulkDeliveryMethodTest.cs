using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ContactNotifyModes = Enterprise.Core.Constants.ContactNotifyModes;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestsSubclassesOf(typeof(BulkDeliveryMethod))]
	public abstract class BulkDeliveryMethodTest<BulkDeliveryMethodT> : TestCaseWithFactory where BulkDeliveryMethodT : BulkDeliveryMethod, new()
	{
		public void TestIsListed()
		{
			AssertEquals(typeof(BulkDeliveryMethodT), List[Method.Code].GetType());
		}

		public void TestUsesPrinter()
		{
			AssertEquals(UsesPrinter, Method.UsesPrinter);
		}

		public void TestAllowCoverNote()
		{
			AssertEquals(AllowCoverNote, Method.AllowCoverNote);
		}

		#region Implementation
		protected abstract bool UsesPrinter { get; }

		protected abstract bool AllowCoverNote { get; }

		public BulkDeliveryMethodT Method
		{
			get
			{
				return method ?? (method = new BulkDeliveryMethodT());
			}
		}

		BulkDeliveryMethodT method;
		public BulkDeliveryMethodList List
		{
			get
			{
				return list ?? (list = new BulkDeliveryMethodList());
			}
		}

		BulkDeliveryMethodList list;
		public DocDeliveryContact PrintContact
		{
			get
			{
				return printContact ?? (printContact = NewDeliveryContact(ContactNotifyModes.Print));
			}
		}

		DocDeliveryContact printContact;
		public DocDeliveryContact FaxContact
		{
			get
			{
				return faxContact ?? (faxContact = NewDeliveryContact(ContactNotifyModes.Fax));
			}
		}

		DocDeliveryContact faxContact;
		public DocDeliveryContact EmailContact
		{
			get
			{
				return emailContact ?? (emailContact = NewDeliveryContact(ContactNotifyModes.Email));
			}
		}

		DocDeliveryContact emailContact;
		public DeliveryInstructions Instructions
		{
			get
			{
				if (instructions == null)
				{
					instructions = new DeliveryInstructions();
					instructions.PrinterDelivery.PrintQueuePK = ZGuid.NewZGuid();
				}

				return instructions;
			}
		}

		DeliveryInstructions instructions;
		DocDeliveryContact NewDeliveryContact(ZString docsEngineDeliveryMethod)
		{
			DocDeliveryContact result = new DocDeliveryContact(Factory);
			result.DeliveryMethod = docsEngineDeliveryMethod;
			return result;
		}

		#endregion
	}
}
