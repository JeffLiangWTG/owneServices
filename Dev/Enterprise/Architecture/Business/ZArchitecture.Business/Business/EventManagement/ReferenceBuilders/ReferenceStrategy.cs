using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.Business
{
	public class ReferenceStrategy
	{
		public const char VersionChar = 'V';

		static readonly object initializationLock = new object();
		readonly ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();

		public static ReferenceStrategy Instance
		{
			get
			{
				if (_instance != null)
				{
					return _instance;
				}

				lock (initializationLock)
				{
					if (_instance == null)
					{
						_instance = new ReferenceStrategy();
					}
				}
				return _instance;
			}
		}
		[ThreadSafe(ThreadSafeAttribute.Mechanism.Lock)]
		static ReferenceStrategy _instance;

		readonly IDictionary<Type, IDictionary<int, object>> Parsers = new Dictionary<Type, IDictionary<int, object>>();

		public void Deserialise<T>(T data, string reference) where T : IReferenceVersion
		{
			var version = data.GetVersion(reference);
			Deserialise(data, reference, version);
		}

		public void Deserialise<T>(T data, string reference, int version) where T : IReferenceVersion
		{
			var builder = GetParser<T>(version);
			builder.Deserialise(data, reference);
			data.Version = builder.Version;
		}

		public string Serialise<T>(T data) where T : IReferenceVersion
		{
			return GetParser<T>(data.Version).Serialise(data);
		}

		IDictionary<int, object> GetRelatedParsers<T>() where T : IReferenceVersion
		{
			try
			{
				readerWriterLock.EnterReadLock();

				Type referenceType = typeof(T);
				if (Parsers.TryGetValue(referenceType, out var parsers))
				{
					return parsers;
				}

				readerWriterLock.ExitReadLock();
				readerWriterLock.EnterWriteLock();

				if (Parsers.TryGetValue(referenceType, out parsers))
				{
					return parsers;
				}

				parsers = typeof(ReferenceStrategy).Assembly
					.GetTypes()
					.Where(type => typeof(IReferenceParser<T>).IsAssignableFrom(type))
					.Where(type =>
						!type.IsAbstract &&
						type.GetConstructor(Array.Empty<Type>()) != null)
					.Select(type => Activator.CreateInstance(type))
					.ToDictionary(b => ((IReferenceParser<T>)b).Version);

				if (!parsers.Any())
				{
					parsers = null;
				}

				Parsers.Add(referenceType, parsers);

				return parsers;
			}
			finally
			{
				if (readerWriterLock.IsWriteLockHeld)
				{
					readerWriterLock.ExitWriteLock();
				}
				else if (readerWriterLock.IsReadLockHeld)
				{
					readerWriterLock.ExitReadLock();
				}
			}
		}

		IReferenceParser<T> GetParser<T>(int version) where T : IReferenceVersion
		{
			var versions = GetRelatedParsers<T>()
				?? throw new ArgumentException(Logs.StratergyDoesNotExist(typeof(T)));
			if (!versions.ContainsKey(version))
			{
				throw new ArgumentException(Logs.StratergyNotFound(version));
			}
			return ((IReferenceParser<T>)versions[version]);
		}

		public static class Logs
		{
			public static string StratergyDoesNotExist(Type type)
			{
				return $"Reference Stratergy type does not exist, Type: {type}"; // English Error Message
			}

			public static string StratergyNotFound(int version)
			{
				return $"No Reference Stratergy found, Version: {version}"; // English Error Message
			}
		}
	}
}
