using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(MessageSendingActionParent))]
	sealed class MessageSendingActionParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestWhenMultipleActionsAreSelected()
		{
			JobHeader.CHGTSTCusTempStorageDecs.AddNew();
			JobHeader.CHGTSTCusTempStorageDecs.AddNew();
			var parent = new MessageSendingActionParent(JobHeader, JobHeader.CHGTSTCusTempStorageDecs.Cast<CusTempStorageDec>(), x => "REF", null);
			AssertEquals(2, parent.SendingObjectsCollection.Count);

			parent.SendingObjectsCollection[0].ShouldSend = true;
			parent.SendingObjectsCollection[1].ShouldSend = true;
			parent.RunPreSaveValidation();
			AssertHasRowErrorContaining(parent, "Please select only one declaration to send.");

			parent.SendingObjectsCollection[0].ShouldSend = true;
			parent.SendingObjectsCollection[1].ShouldSend = false;
			parent.RunPreSaveValidation();
			AssertNoRowErrorContaining(parent, "Please select only one declaration to send.");
		}

		public void TestMessageErrorCollector()
		{
			JobHeader.SJH_OH_Customer = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;

			var chgtestDec = JobHeader.CHGTSTCusTempStorageDecs.AddNew();
			chgtestDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			chgtestDec.RunPreSaveValidation();
			Assert("Precondition", chgtestDec.HasMessageErrors);
			Assert("Precondition", chgtestDec.NewCustodianBranchInfo.HasMessageErrors());

			var chgoffDec = JobHeader.CHGOFFCusTempStorageDecs.AddNew();
			chgoffDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			chgoffDec.CusTempStorageLines.AddNew();
			chgoffDec.RunPreSaveValidation();
			Assert("Precondition", chgoffDec.HasMessageErrors);
			var parent = new MessageSendingActionParent(JobHeader, JobHeader.CHGOFFCusTempStorageDecs.Cast<CusTempStorageDec>(), x => "REF", null);
			parent.SendingObjectsCollection[0].ShouldSend = true;

			AssertContains("CHGOFF Decs' message errors are collected and displayed", chgoffDec.STH_OwnerReferenceNumberInfo.HumanReadableName + ": " + chgoffDec.STH_OwnerReferenceNumberInfo.GetMessageErrors().ToUniqueMessageListString(), parent.BizObjValidationMessageErrors);
			AssertNotContains("Other types of Decs' message errors are NOT collected and displayed", chgtestDec.NewCustodianBranchInfo.HumanReadableName + ": " + chgtestDec.NewCustodianBranchInfo.GetMessageErrors().ToUniqueMessageListString(), parent.BizObjValidationMessageErrors);
		}

		public void TestMessageSendingObjectProperty()
		{
			var parent = new MessageSendingActionParent(JobHeader, JobHeader.CHGTSTCusTempStorageDecs.Cast<CusTempStorageDec>(), x => "REF", null);
			AssertEquals(1, parent.MessageSendingObjectProperties.Count());

			var property = parent.MessageSendingObjectProperties.First();
			AssertEquals(true, property.IsMandatory);
			AssertEquals("Details", property.PropertyName);
			AssertEquals(200, property.ColumnWidth);
		}

		public void TestAdditionalWarnings()
		{
			JobHeader.CHGTSTCusTempStorageDecs.AddNew();
			var parent = new MessageSendingActionParentForTest(JobHeader, JobHeader.CHGTSTCusTempStorageDecs.Cast<CusTempStorageDec>(), x => "REF", null);
			parent.WarningToAdd = "this is a warning";
			parent.SendingObjectsCollection[0].ShouldSend = true;
			AssertEquals("this is a warning", parent.AdditionalWarnings);
		}

		public void TestMessageSendingValidation()
		{
			var parent = new MessageSendingActionParent(JobHeader, JobHeader.CHGTSTCusTempStorageDecs.Cast<CusTempStorageDec>(), x => "REF", null);
			var validation = parent.MessageSendingValidation;
			AssertType<MessageSendingValidation>(validation);
		}

		protected override BusinessObject GetNewBusinessObject() => new MessageSendingActionParent(JobHeader, JobHeader.CHGTSTCusTempStorageDecs.Cast<CusTempStorageDec>(), x => "REF", null);

		CusTempStorageJobHeader jobHeader;
		CusTempStorageJobHeader JobHeader => jobHeader ?? (jobHeader = Factory.New<CusTempStorageJobHeader>());

		sealed class MessageSendingActionParentForTest : MessageSendingActionParent
		{
			public MessageSendingActionParentForTest(BusinessObject topBusinessObject, IEnumerable<BusinessObject> messagingEntities, Func<BusinessObject, ZString> getDetails, SecurityCheckpoint securityCheckpointToSendWithMessageError) : base(topBusinessObject, messagingEntities, getDetails, securityCheckpointToSendWithMessageError)
			{
			}

			public string WarningToAdd;

			protected override ZString GetAdditionalWarningsCore() => WarningToAdd ?? base.GetAdditionalWarnings();
		}
	}
}
