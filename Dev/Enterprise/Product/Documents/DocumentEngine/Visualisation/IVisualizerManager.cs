using System.Collections.Generic;
namespace Enterprise.DocumentEngine.Visualisation
{
	public interface IVisualizerManager
	{
		IEnumerable<Report> Reports { get; }

		void SaveData();
		void ClearData();
		void RevertData();
	}
}