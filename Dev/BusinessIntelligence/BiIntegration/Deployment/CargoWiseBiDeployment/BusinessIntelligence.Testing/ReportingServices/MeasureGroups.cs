namespace CargoWise.Bi.BusinessIntelligence.Testing.ReportingServices
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;

	public class MeasureGroups : IEnumerable
	{
		public MeasureGroup this[string measureGroupName]
		{
			get
			{
				return measureGroups.FirstOrDefault(mg => mg.Name.Equals(measureGroupName, StringComparison.OrdinalIgnoreCase));
			}
		}

		public MeasureGroup AddMeasureGroup(string name)
		{
			var measureGroup = new MeasureGroup(name);
			measureGroups.Add(measureGroup);
			return measureGroup;
		}

		public void AddMeasureGroup(MeasureGroup measureGroup)
		{
			measureGroups.Add(measureGroup);
		}

		public IEnumerable<string> MeasureGroupNames
		{
			get
			{
				var measureGroupNames = measureGroups.Select(x => x.Name).OrderBy(x => x);
				return measureGroupNames;
			}
		}

		readonly List<MeasureGroup> measureGroups = new List<MeasureGroup>();

		#region ICollection methods

		IEnumerator IEnumerable.GetEnumerator()
		{
			return measureGroups.GetEnumerator();
		}

		#endregion
	}

	public class MeasureGroup : IEnumerable
	{
		public MeasureGroup(string name)
		{
			Name = name;
		}
		public readonly string Name;

		public Measure AddMeasure(string name, string expression)
		{
			var measure = new Measure(name, expression);
			measures.Add(measure);
			return measure;
		}

		public void AddMeasure(Measure measure)
		{
			measures.Add(measure);
		}

		public Measure this[string measureName]
		{
			get
			{
				return measures.FirstOrDefault(m => m.Name.Equals(measureName, StringComparison.OrdinalIgnoreCase));
			}
		}

		public IEnumerable<string> MeasureNames
		{
			get
			{
				return measures.Select(m => m.Name).OrderBy(m => m);
			}
		}

		readonly List<Measure> measures = new List<Measure>();

		#region ICollection methods

		IEnumerator IEnumerable.GetEnumerator()
		{
			return measures.GetEnumerator();
		}

		#endregion
	}

	public class Measure
	{
		public Measure(string name, string expression)
		{
			Name = name;
			Expression = expression;
		}
		public readonly string Name;
		public readonly string Expression;
	}
}
