using System;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface IDocDataObjectOnValueChangedActionBuilder
	{
		void Validate(params string[] propertyNames);
		void Do(Action action);
	}
}
