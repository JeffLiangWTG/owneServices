using System;

namespace Enterprise.Customs.CA.Business
{
	internal interface IExposeMethodsForPGADataLoad
	{
		void AddToDisposableList(IDisposable disposable);
		bool HasColumn(string columnName);
		void DisplayLogMessage(string message);
	}
}
