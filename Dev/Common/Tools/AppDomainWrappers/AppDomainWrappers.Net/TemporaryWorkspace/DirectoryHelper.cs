using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Threading;

namespace AppDomainWrappers.Net
{
	public static class DirectoryHelper
	{
#pragma warning disable CW1021 // Static Fields Are Thread Static Rule - Making use of a lock.
		static List<int> shuffledNumbers;
		static int currentMin;
		static int currentMax;

		static int currentIndex;
		static readonly object lockObject = new(); // Lock for thread safety
		static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create(); // Secure RNG
#pragma warning restore CW1021 // Static Fields Are Thread Static Rule - Making use of a lock.

		public static string GenerateRandomNumber(int minValue, int maxValue)
		{
			lock (lockObject) // Ensure only one thread can access this block at a time
			{
				if (shuffledNumbers == null || minValue != currentMin || maxValue != currentMax || currentIndex >= shuffledNumbers.Count)
				{
					InitializeShuffledNumbers(minValue, maxValue);
				}

				return GetNextUniqueRandomNumber();
			}
		}

		public static void InitializeShuffledNumbers(int minValue, int maxValue)
		{
			if (minValue >= maxValue)
			{
				throw new ArgumentException("minValue must be less than maxValue.");
			}

			// Set the current min and max static values.
			currentMin = minValue;
			currentMax = maxValue;

			// Generate a list of numbers in the specified range
			shuffledNumbers = Enumerable.Range(minValue, maxValue - minValue + 1).ToList();

			// Fisher-Yates shuffle using RandomNumberGenerator for secure randomization
			for (var i = shuffledNumbers.Count - 1; i > 0; i--)
			{
				var j = GetRandomNumber(0, i + 1); // Get a secure random index
												   // Swap shuffledNumbers[i] with shuffledNumbers[j]
				(shuffledNumbers[j], shuffledNumbers[i]) = (shuffledNumbers[i], shuffledNumbers[j]);
			}

			currentIndex = 0; // Reset index after shuffling
		}

		static string GetNextUniqueRandomNumber()
		{
			// Return the next number in the shuffled list
			var uniqueNumber = shuffledNumbers[currentIndex];
			currentIndex++;
			return uniqueNumber.ToString();
		}

		static int GetRandomNumber(int minValue, int maxValue)
		{
			if (minValue >= maxValue)
			{
				throw new ArgumentException("minValue must be less than maxValue.");
			}

			// Calculate the range
			var range = maxValue - minValue;

			// Create a byte array to hold the random value
			var randomBytes = new byte[4]; // 4 bytes for a 32-bit integer
			rng.GetBytes(randomBytes);

			// Convert the byte array to a positive integer
			var randomValue = BitConverter.ToInt32(randomBytes, 0) & 0x7FFFFFFF; // Mask to get a positive number

			// Scale the random value to the desired range
			return minValue + (randomValue % range);
		}

		public static string GetDirectoryName(string path)
		{
			// Handle case where the path is just a single directory or file name
			if (!Path.IsPathRooted(path) && !path.Contains(Path.DirectorySeparatorChar.ToString()) && !path.Contains(Path.AltDirectorySeparatorChar.ToString()))
			{
				return path;
			}

			// Get the directory path if it's a file, or use the directory path as-is
			var directoryPath = Path.GetDirectoryName(path) ?? path;

			// Return the last part of the directory path (directory name)
			return Path.GetFileName(directoryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
		}

		public static DirectoryInfo CreateLowSecurityDirectory(string path)
		{
			// Define directory security settings
			var directorySecurity = new DirectorySecurity();

			// Add rule to allow full control for Everyone.
#pragma warning disable CW1161 // Res.GetString Analyzer - System Constant.
			directorySecurity.AddAccessRule(
				new FileSystemAccessRule(
					"Everyone",
					FileSystemRights.FullControl,
					InheritanceFlags.None,
					PropagationFlags.NoPropagateInherit,
					AccessControlType.Allow));
#pragma warning restore CW1161 // Res.GetString Analyzer - System Constant.

			// Create the directory with the specified security settings
			return Directory.CreateDirectory(path, directorySecurity);
		}

		public static void DeleteDirectoryContents(DirectoryInfo directoryInfo)
		{
			if (!directoryInfo.Exists)
			{
				return;
			}

			// Get all files and delete them
			foreach (var file in directoryInfo.GetFiles())
			{
				file.Delete();
			}

			// Get all subdirectories and recursively delete their contents
			foreach (var subDir in directoryInfo.GetDirectories())
			{
				DeleteDirectoryContents(subDir); // Recursively clean subdirectories
				_ = TryDeleteDirectory(subDir); // Delete the now empty subdirectory
			}

			_ = TryDeleteDirectory(directoryInfo);  // Now Delete this top level.
		}

		static bool TryDeleteDirectory(DirectoryInfo directoryInfo, int maxRetries = 15, int delay = 100)
		{
			if (directoryInfo == null)
			{
				throw new ArgumentNullException(nameof(directoryInfo));
			}

			if (maxRetries < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(maxRetries));
			}

			if (delay < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(delay));
			}

			for (var i = 0; i < maxRetries; ++i)
			{
				try
				{
					if (directoryInfo.Exists)
					{
						Directory.Delete(directoryInfo.FullName);
					}

					return true;
				}
				catch (IOException)
				{
					Thread.Sleep(delay);
				}
				catch (UnauthorizedAccessException)
				{
					Thread.Sleep(delay);
				}
			}

			return false;
		}
	}
}
