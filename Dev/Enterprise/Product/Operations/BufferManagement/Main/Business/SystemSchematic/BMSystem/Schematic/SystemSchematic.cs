using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class SystemSchematic
	{
		public SystemSchematic(BMSystem system)
		{
			this.system = system;
		}

		readonly BMSystem system;

		public string SchematicText
		{
			get { return GetSchematicText(); }
		}

		#region Schematic Calculation

		string GetSchematicText()
		{
			var result = new StringBuilder();

			var specs = GetSystemSpec();
			if (specs.Count > 0)
			{
				var minimumOffset = specs.OrderBy(s => s.OffsetHours).First().OffsetHours;
				var longestNameLength = specs.OrderByDescending(s => s.Name.Length).First().Name.Length;

				foreach (var spec in specs)
				{
					result.AppendLine(GetLineForSpec(spec, minimumOffset, longestNameLength));
				}
			}

			return result.ToString();
		}

		List<ComponentSchematicSpec> GetSystemSpec()
		{
			var specs = new List<ComponentSchematicSpec>();

			foreach (var component in system.Components.Where(c => c.FC_IsActive).OrderBy(c => c.FC_DisplaySequence))
			{
				specs.Add(new ComponentSchematicSpec(component));
				foreach (var subComponent in component.ChildComponents.Where(c => c.FC_IsActive).OrderBy(c => c.FC_DisplaySequence))
				{
					specs.Add(new ComponentSchematicSpec(subComponent));
				}
			}

			return specs;
		}

		string GetLineForSpec(ComponentSchematicSpec spec, int minimumOffset, int longestNameLength)
		{
			var name = spec.Name + new string(' ', longestNameLength - spec.Name.Length) + ": ";
			var leadingSpaces = new string(' ', (minimumOffset * -1 + spec.OffsetHours));
			string componentContent;

			switch (spec.Type)
			{
				case BMComponentTypeList.Codes.Bucket:
					componentContent = BucketBase;
					break;

				case BMComponentTypeList.Codes.Buffer:
					componentContent = "     " + new string(BufferBase, spec.TimeSpanHours);
					break;

				case BMComponentTypeList.Codes.Constraint:
					componentContent = "   " + ConstraintBase;
					break;

				case BMComponentTypeList.Codes.Decouple:
					componentContent = DecoupleBase;
					break;

				default:
					componentContent = "?";
					break;
			}

			return name + leadingSpaces + componentContent;
		}

		const string BucketBase = @"\__.__/";
		const char BufferBase = '-';
		const string ConstraintBase = "(X)"; // Represents a component
		const string DecoupleBase = "=={}==";

		#endregion

		#region ComponentSchematicSpec

		class ComponentSchematicSpec
		{
			internal ComponentSchematicSpec(BMComponent component)
			{
				var sequenceNumber = string.Format(CultureInfo.InvariantCulture, "{0}. ", component.FC_DisplaySequence);
				if (component.IsChildComponent)
				{
					sequenceNumber = "  " + sequenceNumber;
				}

				Name = sequenceNumber + component.FC_Name;
				Type = component.FC_Type;
				TimeSpanHours = (int)component.BufferTimeSpanHours;
				OffsetHours = component.FC_OffsetInMinutes / 60;
			}

			internal string Name { get; private set; }
			internal string Type { get; private set; }

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
			internal int TimeSpanHours { get; private set; }

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
			internal int OffsetHours { get; private set; }
		}

		#endregion

		#region Legend Schematic

		public static string TextLegendSchematic
		{
			get
			{
				return string.Format(CultureInfo.InvariantCulture,
(NoResString)@"Bucket:		{0}

Buffer:		{1}

Constraint:	{2}

Decouple:	{3}", // System schematic terms
					/*0*/ BucketBase,
					/*1*/ new string(BufferBase, 7),
					/*2*/ ConstraintBase,
					/*3*/ DecoupleBase);
			}
		}

		#endregion
	}
}
