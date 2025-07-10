using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(RecipientRole))]
	class RecipientRoleTest : DataObjectTestCase<RecipientRole>
	{
		public void TestStaticConstructorFillsInCodeAndDescription()
		{
			var recipient = RecipientRole.New(new RecipientRoleDetail() { Type = RecipientRoleType.BRO });
			AssertEquals(RecipientRoleType.BRO, recipient.Code);
			AssertEquals("Broker", recipient.Description);
			AssertNull("ServiceCode", recipient.ServiceCode);
			AssertNull("ServiceDescription", recipient.ServiceDescription);

			recipient = RecipientRole.New(new RecipientRoleDetail() { Type = RecipientRoleType.FOR });
			AssertEquals(RecipientRoleType.FOR, recipient.Code);
			AssertEquals("Forwarder", recipient.Description);
			AssertNull("ServiceCode", recipient.ServiceCode);
			AssertNull("ServiceDescription", recipient.ServiceDescription);

			recipient = RecipientRole.New(new RecipientRoleDetail() { Type = RecipientRoleType.BRO, ServiceCode = ServiceCodeType.HLD });
			AssertEquals(RecipientRoleType.BRO, recipient.Code);
			AssertEquals("Broker", recipient.Description);
			AssertEquals("ServiceCode", ServiceCodeType.HLD, recipient.ServiceCode);
			AssertEquals("ServiceDescription", Enterprise.UniversalDataBuss.Integration.DataObjects.ServiceCodesList.Descriptions.Hold, recipient.ServiceDescription);
		}

		public void TestRecipentRoleTypeListAndEnumAreInSync()
		{
			var enumPlusExcludedValues = new List<string>(Enum.GetNames(typeof(RecipientRoleType)));
			var listGetter = ObjectFactory.New<IAllPossibleRecipientTypesGetter>();

			enumPlusExcludedValues.AddRange(listGetter.GetListOfRecipientsWhichCannotReceiveUniversalXml().Cast<ICodeDescription>().Select(cdp => cdp.Code));

			string enumValues = string.Join("\r\n", enumPlusExcludedValues.OrderBy(o => o).ToArray());
			string listValues = string.Join("\r\n", listGetter.GetRecipientList().Cast<ICodeDescription>().Select(o => o.Code).OrderBy(o => o).ToArray());

			var message =
$@"The contents of {typeof(RecipientRoleType).FullName} enum no longer match the RecipentRoleTypeList, as accessed through {nameof(IAllPossibleRecipientTypesGetter)}.
These enum values may be valid recipients for Universal XML. This would be an external party (an organisation record) which can have Universal XML sent to them via eHub.
*** It should NEVER include email address recipients as opposed to organisations. This would consitute a free way to bypass the paid version of e2e via eHub. ***
If you have added a new organisation-based recipient, consider adding the recipient to {typeof(RecipientRoleType).FullName}, meaning this organisation will be able to receive Universal XML.
Otherwise, exclude the new code in {nameof(IAllPossibleRecipientTypesGetter)}.{nameof(IAllPossibleRecipientTypesGetter.GetListOfRecipientsWhichCannotReceiveUniversalXml)}";

			AssertMultilineASCIIEquals(message, enumValues, listValues);
		}

		public void TestServiceCodeTypeListAndEnumAreInSync()
		{
			var enumValues = string.Join("\r\n", Enum.GetNames(typeof(ServiceCodeType)).OrderBy(o => o).ToArray());
			var listValues = string.Join("\r\n", new Integration.DataObjects.ServiceCodesList().Cast<ICodeDescription>().Select(o => o.Code).OrderBy(o => o).ToArray());
			AssertMultilineASCIIEquals("The enum ServiceCodeType values must be the same as the ServiceCodesList codes.", enumValues, listValues);
		}

		protected override bool ShouldBeFlattenedIntoAttributes
		{
			get { return true; }
		}
	}
}

