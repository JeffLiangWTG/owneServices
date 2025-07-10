using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.Business
{
	public sealed class MessageResponseSemaphoreHelper : IDisposable
	{
		public void CreateSemaphoreForDeclaration(JobDeclaration declaration)
		{
			var semaphore = new MessageResponseSemaphore(declaration);
			semaphoreHandles.Add(EnvProxy.Instance.SemaphoreProvider.CreateSemaphoreHandle(semaphore));
		}

		public static void RemoveSemaphoreForDeclaration(JobDeclaration declaration)
		{
			var semaphore = new MessageResponseSemaphore(declaration);
			EnvProxy.Instance.SemaphoreProvider.RemoveSemaphore(semaphore);
		}

		public static bool SemaphoreExistsForDeclaration(JobDeclaration declaration)
		{
			var semaphore = new MessageResponseSemaphore(declaration);
			var handles = EnvProxy.Instance.SemaphoreProvider.GetActiveSemaphoreHandles(semaphore);
			return handles.Length != 0;
		}

		public static bool SemaphoreExistsForDeclaration(JobDeclaration declaration, out ISemaphoreInfo semaphoreInfo)
		{
			var semaphore = new MessageResponseSemaphore(declaration);
			var handles = EnvProxy.Instance.SemaphoreProvider.GetActiveSemaphoreHandles(semaphore);
			semaphoreInfo = handles.Length != 0 ? handles.OrderBy(x => x.CreateTimeUtc).FirstOrDefault() : null;
			return handles.Length != 0;
		}

		public void Dispose()
		{
			semaphoreHandles.ForEach(s => s.Dispose());
			semaphoreHandles.Clear();
		}

		readonly List<ISemaphoreHandle> semaphoreHandles = new();
	}
}
