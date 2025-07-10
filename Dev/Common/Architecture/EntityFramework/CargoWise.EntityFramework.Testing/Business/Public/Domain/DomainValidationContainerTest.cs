using System.Collections.Generic;
using System.Reflection;
using CargoWise.ComponentModel;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DomainValidationContainerTest : TestCaseWithDummy
	{
		#region Domain Validation Groups

		public void TestMainGroup()
		{
			AssertNotNull(Service.MainGroup);
			AssertEquals("Should be cached", Service.MainGroup, Service.MainGroup);
		}

		public void TestAllValidationGroups()
		{
			List<DomainValidationGroup> groupList = new List<DomainValidationGroup>(Service.AllValidationGroups);
			AssertEquals("Should only have one group by default", 1, groupList.Count);
			AssertEquals("Should be the main group", Service.MainGroup, groupList[0]);

			groupList = new List<DomainValidationGroup>(Service.AllValidationGroups);
			Service.AddAllFrom(Service);
			AssertEquals("Should still have one group as it should not add its own main group", 1, groupList.Count);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			Service.AddAllFrom(newFactory.Validation);
			groupList = new List<DomainValidationGroup>(Service.AllValidationGroups);
			AssertEquals("Should still have one group. Should not add the additional validation group if ValidationContainer is not loaded", 1, groupList.Count);

			object lazyLoadValidationGroup = newFactory.Validation.MainGroup;
			Service.AddAllFrom(newFactory.Validation);
			groupList = new List<DomainValidationGroup>(Service.AllValidationGroups);
			AssertEquals("Should have two groups now", 2, groupList.Count);
			AssertEquals("Should be the main group", Service.MainGroup, groupList[0]);
			AssertEquals("Should be the NewDomain's main group", newFactory.Validation.MainGroup, groupList[1]);

			BusinessObjectFactory otherNewFactory = new BusinessObjectFactory();
			lazyLoadValidationGroup = otherNewFactory.Validation.MainGroup;
			Service.AddAllFrom(otherNewFactory.Validation);
			groupList = new List<DomainValidationGroup>(Service.AllValidationGroups);
			AssertEquals("Should have three groups now", 3, groupList.Count);
		}

		public void TestAddAllFrom()
		{
			List<DomainValidationGroup> groupList = new List<DomainValidationGroup>(Service.AllValidationGroups);
			AssertEquals("Pre-condition", 1, groupList.Count);

			BusinessObjectFactory newFactoryWithAdditionalGroup = new BusinessObjectFactory();
			object lazyLoadMainGroup = newFactoryWithAdditionalGroup.Validation.MainGroup;
			BusinessObjectFactory newFactoryForAdditionalGroup = new BusinessObjectFactory();
			lazyLoadMainGroup = newFactoryForAdditionalGroup.Validation.MainGroup;
			newFactoryWithAdditionalGroup.Validation.AddAllFrom(newFactoryForAdditionalGroup.Validation);
			Service.AddAllFrom(newFactoryWithAdditionalGroup.Validation);
			groupList = new List<DomainValidationGroup>(Service.AllValidationGroups);
			AssertEquals("Should have three groups now", 3, groupList.Count);
			AssertEquals("Should be the main group", Service.MainGroup, groupList[0]);
			AssertEquals("Should be the NewDomainWithAdditionalGroup's main group", newFactoryWithAdditionalGroup.Validation.MainGroup, groupList[1]);
			AssertEquals("Should be the NewDomainForAdditionalGroup's main group", newFactoryForAdditionalGroup.Validation.MainGroup, groupList[2]);
		}

		public void TestAddAdditionalValidationGroup_ShouldNotBeAddingSameGroup()
		{
			List<DomainValidationGroup> groupList = new List<DomainValidationGroup>(Service.AllValidationGroups);
			AssertEquals("Pre-condition", 1, groupList.Count);

			BusinessObjectFactory otherFactory1 = new BusinessObjectFactory();
			object lazyLoadValidationGroup = otherFactory1.Validation.MainGroup;
			Service.AddAllFrom(otherFactory1.Validation);
			groupList = new List<DomainValidationGroup>(Service.AllValidationGroups);
			AssertEquals("Should have two groups now", 2, groupList.Count);

			BusinessObjectFactory otherDomain2 = new BusinessObjectFactory();
			lazyLoadValidationGroup = otherDomain2.Validation.MainGroup;
			otherDomain2.Validation.AddAllFrom(otherFactory1.Validation);
			Service.AddAllFrom(otherDomain2.Validation);
			groupList = new List<DomainValidationGroup>(Service.AllValidationGroups);
			AssertEquals("Should only have three groups rather than four. OtherDomain2.AdditionalGroup should not be added as it was already added from OtherDomain1's Main Group", 3, groupList.Count);
			AssertEquals("Should be the main group", Service.MainGroup, groupList[0]);
			AssertEquals("Should be OtherDomain1's main group", otherFactory1.Validation.MainGroup, groupList[1]);
			AssertEquals("Should be OtherDomain2's main group", otherDomain2.Validation.MainGroup, groupList[2]);
		}

		[ExpectNoExceptions]
		public void TestAddAdditionalValidationGroup_NullParam()
		{
			Service.AddAllFrom(null);
		}

		public void TestHasDomainValidation()
		{
			Assert("Main group and additional group are not loaded, should be false", !Service.HasDomainValidation);

			object lazyLoadMainGroup = Service.MainGroup;
			Assert("Main group loaded, should be true", Service.HasDomainValidation);

			service = null;
			Assert("Main group and additional groups are not loaded, should be false", !Service.HasDomainValidation);

			LazyLoadAdditionalGroups(Service);
			Assert("Additional groups loaded but nothing in the list, should be false", !Service.HasDomainValidation);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			lazyLoadMainGroup = newFactory.Validation.MainGroup;
			Service.AddAllFrom(newFactory.Validation);
			Assert("Additional groups loaded and has one element in the list, should be true", Service.HasDomainValidation);
		}

		public void TestHasDomainValidation_WhenValidationRequestedFired()
		{
			AssertEquals("False when ValidationRequested not hooked", false, Service.HasDomainValidation);
			Service.ValidationRequested += delegate
			{ };
			AssertEquals("True when ValidationRequested hooked", true, Service.HasDomainValidation);
		}

		#endregion

		#region ValidationRequested event

		public void TestValidationRequestedEvent()
		{
			Factory.Validation.ValidationRequested += new ValidationRequestedEventHandler(Validation_ValidationRequested);
			AssertEquals("No validation error initially", false, Dummy.Z0_DescriptionInfo.HasErrors());
			Dummy.Z0_Description = "error";
			AssertEquals("Error in property", true, Dummy.Z0_DescriptionInfo.HasErrors());
			Dummy.Z0_Description = "error but cancel it in event";
			AssertEquals("No errors when event was cancelled", false, Dummy.Z0_DescriptionInfo.HasErrors());
		}

		void Validation_ValidationRequested(object sender, ValidationRequestedEventArgs e)
		{
			if (e.Property == "Z0_Description" && e.Property.Value.ToString().Contains("cancel"))
			{
				e.Cancel = true;
			}
		}

		#endregion

		#region Implementation

		ValidationDomainService Service
		{
			get
			{
				if (service == null)
				{
					service = ValidationDomainService.Get(new BusinessObjectFactory());
				}
				return service;
			}
		}
		ValidationDomainService service;

		void LazyLoadAdditionalGroups(ValidationDomainService validationContainer)
		{
			PropertyInfo property = validationContainer.GetType().GetProperty("AdditionalGroups", BindingFlags.Instance | BindingFlags.NonPublic);
			property.GetAccessors(true)[0].Invoke(validationContainer, null);
		}

		#endregion
	}
}
