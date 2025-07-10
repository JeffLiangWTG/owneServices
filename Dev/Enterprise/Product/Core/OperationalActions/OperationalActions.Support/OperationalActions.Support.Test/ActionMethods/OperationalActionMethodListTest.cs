using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Support.Testing
{
	internal sealed class OperationalActionMethodListTest : TestCase
	{
		readonly ZGuid WithGUI = new ZGuid("{E7D84DBC-367F-4778-BB87-1A48AB8314C2}");
		readonly ZGuid WithLargeGUI = new ZGuid("{4A121928-CEEA-4F37-9E5D-5FB0BB094F14}");
		readonly ZGuid WithOversizedGUI = new ZGuid("{8F5E3CD2-8366-4E74-8DE7-09FE6E5E8946}");
		readonly ZGuid WithoutGUI = new ZGuid("{2B696C51-0243-4ba2-A7C1-1A43B56DE2F8}");
		readonly ZGuid WithoutUIText = new ZGuid("{c602bd5b-ac5b-43ea-99fc-0abc95b61b91}");
		public void TestIndexes()
		{
			OperationalActionMethodList list = new OperationalActionMethodList(ActionSupporter);
			AssertNull("precondition: with gui (id)", list.GetOperationalActionMethod(ActionMethodProviderIDs.DummyWithMethods, WithGUI));
			AssertNull("precondition: without gui (id)", list.GetOperationalActionMethod(ActionMethodProviderIDs.DummyWithMethods, WithoutGUI));
			AssertNull("precondition: with gui (name)", list.GetOperationalActionMethod(ActionMethodProviderIDs.DummyWithMethods.Guid, WithGUI));
			AssertNull("precondition: without gui (name)", list.GetOperationalActionMethod(ActionMethodProviderIDs.DummyWithMethods.Guid, WithoutGUI));
			list.Add(ActionMethodProviderIDs.DummyWithMethods);
			AssertNotNull("with gui (id)", list.GetOperationalActionMethod(ActionMethodProviderIDs.DummyWithMethods, WithGUI));
			AssertNotNull("without gui (id)", list.GetOperationalActionMethod(ActionMethodProviderIDs.DummyWithMethods, WithoutGUI));
			AssertNotNull("with gui (name)", list.GetOperationalActionMethod(ActionMethodProviderIDs.DummyWithMethods.Guid, WithGUI));
			AssertNotNull("without gui (name)", list.GetOperationalActionMethod(ActionMethodProviderIDs.DummyWithMethods.Guid, WithoutGUI));
		}

		public void TestGetIdEnumerable()
		{
			OperationalActionMethodList list = new OperationalActionMethodList(ActionSupporter);
			AssertContainsExactElementsInAnyOrder("precondition:", Array.Empty<ActionMethodProviderID>(), list.GetAllIds());
			list.Add(ActionMethodProviderIDs.DummyWithMethods);
			list.Add(ActionMethodProviderIDs.DummyWithoutMethods);
			AssertContainsExactElementsInAnyOrder("Should only enumerate elements with methods", new ActionMethodProviderID[] { ActionMethodProviderIDs.DummyWithMethods }, list.GetAllIds());
		}

		public void TestGetMethodEnumerable()
		{
			OperationalActionMethodList list = new OperationalActionMethodList(ActionSupporter);
			AssertContainsExactElementsInAnyOrder("precondition:", Array.Empty<ZGuid>(), MethodDIs(list.GetMethods(ActionMethodProviderIDs.DummyWithMethods)));
			list.Add(ActionMethodProviderIDs.DummyWithMethods);
			AssertContainsExactElementsInAnyOrder("precondition:", new ZGuid[] { WithGUI, WithLargeGUI, WithOversizedGUI, WithoutGUI, WithoutUIText }, MethodDIs(list.GetMethods(ActionMethodProviderIDs.DummyWithMethods)));
		}

		#region ActionSupporter
		IEnumerable<ZGuid> MethodDIs(IEnumerable<OperationalActionMethod> methods)
		{
			foreach (OperationalActionMethod method in methods)
			{
				yield return method == null ? ZGuid.Empty : method.MethodID;
			}
		}

		OperationalActionSupporter ActionSupporter
		{
			get
			{
				return actionSupporter ?? (actionSupporter = new DummyActionSupporter());
			}
		}

		OperationalActionSupporter actionSupporter;
		class DummyActionSupporter : OperationalActionSupporter
		{
			public override BusinessContext BusinessContext
			{
				get
				{
					return (BusinessContext)(-1);
				}
			}

			public override SecurityCheckpoint BaseCheckpoint => Env.Security.None;
			public override Type RootType
			{
				get
				{
					return typeof(DummyBusinessObject);
				}
			}
		}
		#endregion
	}
}
