using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business.Test
{
	class SystemSchematicTest : BMSTestCaseWithFactory
	{
		public void TestLegendSchematic()
		{
			AssertMultilineASCIIEquals("Legend schematic", ExpectedLegendSchematic, SystemSchematic.TextLegendSchematic);
		}

		public void TestSchematic_ShouldIgnoreInactiveComponents()
		{
			var system = CreateSystem();
			var buffer1 = CreateBuffer(system, "buffer1", sequence: 1);
			var buffer2 = CreateBuffer(system, "buffer2", sequence: 2);
			var subBuffer1 = CreateSubBuffer(buffer2, sequence: 1);
			var subBuffer2 = CreateSubBuffer(buffer2, sequence: 2);

			buffer1.FC_IsActive = subBuffer1.FC_IsActive = false;

			AssertMultilineASCIIEquals("",
@"2. buffer2     :      ------------------------------------------------------------------------------------------------
  2. sub-buffer:      ------------------------------------------------------------------------------------------------", new SystemSchematic(system).SchematicText);
		}

		public void TestFlairSchematic()
		{
			var system = GetFlairSystemSchematic(Factory);
			var schematic = new SystemSchematic(system);

			AssertMultilineASCIIEquals("Flair schematic", ExpectedFlairTextSchematic, schematic.SchematicText);
		}

		public static BMSystem GetFlairSystemSchematic(BusinessObjectFactory factory)
		{
			var system = BMSTestHelper.CreateSystem(factory);
			system.FS_Name = "Flair";

			var component1_Future = CreateBucket(system, "Future", sequence: 1);
			var component2_ReadyToRelease = CreateBucket(system, "Ready to Release", sequence: 2);

			var component3_ManufacturingTops = CreateBuffer(system, "Manufacturing Tops", timespanMinutes: 24 * 60, sequence: 3);
			var component3_1_VacuumForming = CreateSubBuffer(component3_ManufacturingTops, "Vacuum Forming", timespanMinutes: 9 * 60, offsetMinutes: 0, sequence: 1);
			var component3_3_Skins = CreateDecouple(component3_ManufacturingTops, "Skins (R)", offsetMinutes: 9 * 60, sequence: 3);
			var component3_4_TopPlant = CreateSubBuffer(component3_ManufacturingTops, "Top Plant", timespanMinutes: 9 * 60, offsetMinutes: 9 * 60, sequence: 4);

			var component4_ManufacturingCarcasses = CreateBuffer(system, "Manufacturing Carcasses", timespanMinutes: 24 * 60, sequence: 4);
			var component4_1_MachiningComponents = CreateSubBuffer(component4_ManufacturingCarcasses, "Machining Components", timespanMinutes: 96 * 60, offsetMinutes: -96 * 60, sequence: 1);
			var component4_2_Components = CreateDecouple(component4_ManufacturingCarcasses, "Components (R)", offsetMinutes: 0, sequence: 2);
			var component4_3_PreFrontFormats = CreateSubBuffer(component4_ManufacturingCarcasses, "Pre-Front Formats Buffer", timespanMinutes: 8 * 60, offsetMinutes: 0, sequence: 3);
			var component4_4_FrontFormats = CreateConstraint(component4_ManufacturingCarcasses, "Front Formats", offsetMinutes: 8 * 60, sequence: 4);
			var component4_5_CarcassFormats = CreateSubBuffer(component4_ManufacturingCarcasses, "Carcass Formats / Assembly Line", timespanMinutes: 16 * 60, offsetMinutes: 8 * 60, sequence: 5);

			var component5_ReadyToShip = CreateBucket(system, "Ready to Ship", sequence: 5, offsetMinutes: 24 * 60);
			var component6_Completed = CreateBucket(system, "Completed", sequence: 6, offsetMinutes: 24 * 60);

			return system;
		}

		const string ExpectedFlairTextSchematic =
@"1. Future                                :                                                                                                 \__.__/
2. Ready to Release                      :                                                                                                 \__.__/
3. Manufacturing Tops                    :                                                                                                      ------------------------
  1. Vacuum Forming                      :                                                                                                      ---------
  3. Skins (R)                           :                                                                                                          =={}==
  4. Top Plant                           :                                                                                                               ---------
4. Manufacturing Carcasses               :                                                                                                      ------------------------
  1. Machining Components                :      ------------------------------------------------------------------------------------------------
  2. Components (R)                      :                                                                                                 =={}==
  3. Pre-Front Formats Buffer            :                                                                                                      --------
  4. Manufacturing CarcassesFront Formats:                                                                                                            (X)
  5. Carcass Formats / Assembly Line     :                                                                                                              ----------------
5. Ready to Ship                         :                                                                                                                         \__.__/
6. Completed                             :                                                                                                                         \__.__/
";

		const string ExpectedLegendSchematic =
@"Bucket:		\__.__/

Buffer:		-------

Constraint:	(X)

Decouple:	=={}==";
	}
}
