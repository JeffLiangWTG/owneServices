using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Areas;

namespace Enterprise.DocumentEngine.Visualisation.Testing
{
	sealed class AreaVisualisationManagerTest : TestCaseWithFactory
	{
		public void TestConstructorParametersAllCheckedForNulls()
		{
			AssertExceptionThrown("Argument [Area area] must not be null.", typeof(ArgumentNullException), delegate
			{ new AreaVisualisationManager(null, delegate { return null; }); });
			Area area = new AreaForTesting();
			AssertExceptionThrown("Argument [RendererGeneralGetter rendererGetter] must not be null.", typeof(ArgumentNullException), delegate
			{ new AreaVisualisationManager(area, null); });
			AreaVisualisationManager manager = new AreaVisualisationManager(area, delegate
			{ return null; });
			AssertExceptionThrown("The delegate passed into the constructor of the AreaVisualisationManager must never return null.", typeof(ArgumentNullException), delegate
			{ RendererGeneral dummy = manager.VisualisationRenderer; });
		}

		public void TestShouldShowInVisualiser()
		{
			Manager.Area.ShouldDelete = false;
			AssertEquals("Manager.ShouldShowInVisualiser", true, Manager.ShouldShowInVisualiser);
			Manager.Area.ShouldDelete = true;
			AssertEquals("Manager.ShouldShowInVisualiser", false, Manager.ShouldShowInVisualiser);

			Manager.ShouldShowInVisualiser = true;
			Manager.Area.ShouldDelete = false;
			AssertEquals("Manager.ShouldShowInVisualiser", true, Manager.ShouldShowInVisualiser);
			Manager.Area.ShouldDelete = true;
			AssertEquals("Manager.ShouldShowInVisualiser", true, Manager.ShouldShowInVisualiser);
		}

		public void TestArea()
		{
			AssertEquals("Manager.Area.GetType()", typeof(AreaForTesting), Manager.Area.GetType());
		}

		public void TestComponents()
		{
			List<VisualiserComponent> list = Manager.Components;
			AssertNotNull(list);
			AssertEquals(list, Manager.Components);
		}

		public void TestVisualisationRenderer()
		{
			RendererGeneralForTesting.HasBeenConstructed = false;
			AssertNotNull("Construct the Manager", Manager);
			AssertEquals("RendererGeneralForTesting.HasBeenConstructed", false, RendererGeneralForTesting.HasBeenConstructed);
			AssertEquals("Manager.VisualisationRenderer.GetType()", typeof(RendererGeneralForTesting), Manager.VisualisationRenderer.GetType());
			AssertEquals("RendererGeneralForTesting.HasBeenConstructed", true, RendererGeneralForTesting.HasBeenConstructed);
		}

		#region Implementation

		AreaVisualisationManager fManager;
		AreaVisualisationManager Manager
		{
			get { return fManager ?? (fManager = GetNewAreaVisualisationManager()); }
		}

		AreaVisualisationManager GetNewAreaVisualisationManager()
		{
			AreaForTesting area = new AreaForTesting();
			return new AreaVisualisationManager(area, delegate
			{ return new RendererGeneralForTesting(area); });
		}

		class AreaForTesting : Area
		{
			public AreaForTesting()
				: base(0, 0, GetReport(), "")
			{
			}

			static Report GetReport()
			{
				return new Report(null, null);
			}

			#region Implementation
			public override Area Clone(int position)
			{
				throw new NotImplementedException();
			}

			public override bool CanCloseAPage
			{
				get { throw new NotImplementedException(); }
			}

			public override ValueProviderDocumenter GetDocumentation()
			{
				throw new System.NotImplementedException();
			}

			public override List<Area> Parents
			{
				get { throw new NotImplementedException(); }
			}
			#endregion

			public RendererGeneral GetMeARendererGeneral()
			{
				return new RendererGeneralForTesting(this);
			}
		}

		class RendererGeneralForTesting : RendererGeneral
		{
			public RendererGeneralForTesting(Area areaToRender)
				: base(areaToRender)
			{
				HasBeenConstructed = true;
			}

			public static bool HasBeenConstructed;
		}
		#endregion
	}
}
