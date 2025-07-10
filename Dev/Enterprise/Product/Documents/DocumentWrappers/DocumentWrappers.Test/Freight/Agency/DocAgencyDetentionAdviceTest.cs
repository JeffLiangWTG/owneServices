using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocAgencyDetentionAdvice))]
	sealed class DocAgencyDetentionAdviceTest : DocumentWrapperTestCase
	{
		public void TestAsAt()
		{
			ZDateTime today = ZDateTime.Today;

			DetentionAdviceHeader advice = new DetentionAdviceHeader(Client);

			advice.AsAt = today;
			AssertEquals(today, advice.AsAt);

			advice.AsAt = today.AddDays(1);
			AssertEquals(today.AddDays(1), advice.AsAt);
		}

		public void TestClient()
		{
			DetentionAdviceHeader advice = new DetentionAdviceHeader(Client);
			DocAgencyDetentionAdvice wrapper = DocAgencyDetentionAdvice.New(advice, Factory);
			AssertEquals("Client", advice, wrapper.WrappedObject);
		}

		public void TestConatiners()
		{
			ZDateTime today = ZDateTime.Today;
			using (AgencyRegistry.Instance.DetentionAdviceWarningDays.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 7))
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 10 }))
			{
				Container11.JC_EmptyReturnedBy = today.AddDays(-1);
				Container12.JC_EmptyReturnedBy = today.AddDays(0);
				Container21.JC_EmptyReturnedBy = today.AddDays(6);
				Container22.JC_EmptyReturnedBy = today.AddDays(7);
				Container23.JC_EmptyReturnedBy = ZDateTime.Empty;

				Movement11.E9_MovementDate = today.AddDays(-1 - 10);
				Movement12.E9_MovementDate = today.AddDays(0 - 10);
				Movement21.E9_MovementDate = today.AddDays(6 - 10);
				Movement22.E9_MovementDate = today.AddDays(7 - 10);
				Movement23.E9_MovementDate = ZDateTime.Empty;

				DetentionAdviceHeader advice = new DetentionAdviceHeader(Client);
				advice.AsAt = today;

				advice.Containers.Add(Container11);
				advice.Containers.Add(Container12);
				advice.Containers.Add(Container21);
				advice.Containers.Add(Container22);
				advice.Containers.Add(Container23);

				advice.Movements.Add(Movement11);
				advice.Movements.Add(Movement12);
				advice.Movements.Add(Movement21);
				advice.Movements.Add(Movement22);
				advice.Movements.Add(Movement23);

				DocAgencyDetentionAdvice wrapper = DocAgencyDetentionAdvice.New(advice, Factory);

				Converter<BusinessObject, string> converter = delegate(BusinessObject bo)
				{
					BillOfLadingContainer container = bo as BillOfLadingContainer;
					if (container != null)
					{
						return container.JC_ContainerNum;
					}

					ContainerMovement movement = bo as ContainerMovement;
					if (movement != null)
					{
						return movement.Stock.R6_ContainerNum;
					}

					return '[' + bo.HumanReadableName + ']';
				};

				CombineAssertions(delegate
				{
					AssertContainsExactElementsInAnyOrder("Overdue Containers",
						converter,
						new BusinessObject[] { Container11, Movement11 },
						Array.ConvertAll(wrapper.Overdue.ToArray<DocumentWrapper>(), (w) => (BusinessObject)w.WrappedObject));

					AssertContainsExactElementsInAnyOrder("Near Due Containers",
						converter,
						new BusinessObject[] { Container12, Container21, Movement12, Movement21 },
						Array.ConvertAll(wrapper.NearDue.ToArray<DocumentWrapper>(), (w) => (BusinessObject)w.WrappedObject));

					AssertContainsExactElementsInAnyOrder("Not Due Containers",
						converter,
						new BusinessObject[] { Container22, Container23, Movement22, Movement23 },
						Array.ConvertAll(wrapper.NotDue.ToArray<DocumentWrapper>(), (w) => (BusinessObject)w.WrappedObject));
				});
			}
		}

		public void TestHasImportContainers()
		{
			DetentionAdviceHeader advice = new DetentionAdviceHeader(Client);
			advice.AsAt = ZDateTime.Today;

			DocAgencyDetentionAdvice wrapper;
			wrapper = DocAgencyDetentionAdvice.New(advice, Factory);

			AssertEquals("No Import Containers", false, wrapper.HasImportContainers);

			advice.Containers.Add(Container11);
			wrapper = DocAgencyDetentionAdvice.New(advice, Factory);
			AssertEquals("Has Import Containers", true, wrapper.HasImportContainers);
		}

		public void TestHasExportContainers()
		{
			DetentionAdviceHeader advice = new DetentionAdviceHeader(Client);
			advice.AsAt = ZDateTime.Today;

			DocAgencyDetentionAdvice wrapper;
			wrapper = DocAgencyDetentionAdvice.New(advice, Factory);

			AssertEquals("No Export Containers", false, wrapper.HasExportContainers);

			advice.Movements.Add(Movement11);
			wrapper = DocAgencyDetentionAdvice.New(advice, Factory);
			AssertEquals("Has Export Containers", true, wrapper.HasExportContainers);
		}

		public void TestBusinessObjectToLogAgainst()
		{
			DetentionAdviceHeader advice = new DetentionAdviceHeader(Client);
			DocAgencyDetentionAdvice wrapper = DocAgencyDetentionAdvice.New(advice, Factory);
			AssertEquals("BusinessObjectToLogAgainst", Client, ((IBODocDataProvider)wrapper).BusinessObjectToLogAgainst);
		}

		#region Implementation

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			DetentionAdviceHeader advice = new DetentionAdviceHeader(Client);
			advice.AsAt = ZDateTime.Today;
			advice.Containers.Add(Container11);

			return new DocumentWrapper[]
			{
				DocAgencyDetentionAdvice.New(advice,Factory),
			};
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			DetentionAdviceHeader advice = new DetentionAdviceHeader(Client);
			advice.AsAt = ZDateTime.Today;
			return DocAgencyDetentionAdvice.New(advice, Factory);
		}

		OrgHeader Client
		{
			get { return client ?? (client = Factory.New<OrgHeader>()); }
		}
		OrgHeader client;

		BillOfLading Bill1
		{
			get { return bill1 ?? (bill1 = Factory.New<BillOfLading>()); }
		}
		BillOfLading bill1;

		BillOfLading Bill2
		{
			get { return bill2 ?? (bill2 = Factory.New<BillOfLading>()); }
		}
		BillOfLading bill2;

		BillOfLadingContainer Container11
		{
			get
			{
				if (container11 == null)
				{
					container11 = Bill1.RealContainers.AddNew();
					container11.JC_ContainerNum = "Container11";
				}
				return container11;
			}
		}
		BillOfLadingContainer container11;

		BillOfLadingContainer Container12
		{
			get
			{
				if (container12 == null)
				{
					container12 = Bill1.RealContainers.AddNew();
					container12.JC_ContainerNum = "Container12";
				}
				return container12;
			}
		}
		BillOfLadingContainer container12;

		BillOfLadingContainer Container21
		{
			get
			{
				if (container21 == null)
				{
					container21 = Bill2.RealContainers.AddNew();
					container21.JC_ContainerNum = "Container21";
				}
				return container21;
			}
		}
		BillOfLadingContainer container21;

		BillOfLadingContainer Container22
		{
			get
			{
				if (container22 == null)
				{
					container22 = Bill2.RealContainers.AddNew();
					container22.JC_ContainerNum = "Container22";
				}
				return container22;
			}
		}
		BillOfLadingContainer container22;

		BillOfLadingContainer Container23
		{
			get
			{
				if (container23 == null)
				{
					container23 = Bill2.RealContainers.AddNew();
					container23.JC_ContainerNum = "Container23";
				}
				return container23;
			}
		}
		BillOfLadingContainer container23;

		RefContainerStock Stock11
		{
			get
			{
				if (stock11 == null)
				{
					stock11 = Factory.New<RefContainerStock>();
					stock11.R6_ContainerNum = "Stock11";
				}
				return stock11;
			}
		}
		RefContainerStock stock11;

		RefContainerStock Stock12
		{
			get
			{
				if (stock12 == null)
				{
					stock12 = Factory.New<RefContainerStock>();
					stock12.R6_ContainerNum = "Stock12";
				}
				return stock12;
			}
		}
		RefContainerStock stock12;

		RefContainerStock Stock21
		{
			get
			{
				if (stock21 == null)
				{
					stock21 = Factory.New<RefContainerStock>();
					stock21.R6_ContainerNum = "Stock21";
				}
				return stock21;
			}
		}
		RefContainerStock stock21;

		RefContainerStock Stock22
		{
			get
			{
				if (stock22 == null)
				{
					stock22 = Factory.New<RefContainerStock>();
					stock22.R6_ContainerNum = "Stock22";
				}
				return stock22;
			}
		}
		RefContainerStock stock22;

		RefContainerStock Stock23
		{
			get
			{
				if (stock23 == null)
				{
					stock23 = Factory.New<RefContainerStock>();
					stock23.R6_ContainerNum = "Stock23";
				}
				return stock23;
			}
		}
		RefContainerStock stock23;

		ContainerMovement Movement11
		{
			get { return movement11 ?? (movement11 = Stock11.Movements.AddNew()); }
		}
		ContainerMovement movement11;

		ContainerMovement Movement12
		{
			get { return movement12 ?? (movement12 = Stock12.Movements.AddNew()); }
		}
		ContainerMovement movement12;

		ContainerMovement Movement21
		{
			get { return movement21 ?? (movement21 = Stock21.Movements.AddNew()); }
		}
		ContainerMovement movement21;

		ContainerMovement Movement22
		{
			get { return movement22 ?? (movement22 = Stock22.Movements.AddNew()); }
		}
		ContainerMovement movement22;

		ContainerMovement Movement23
		{
			get { return movement23 ?? (movement23 = Stock23.Movements.AddNew()); }
		}
		ContainerMovement movement23;

		#endregion
	}
}
